
namespace HospitalUrgencias.Hospital.Services;
using HospitalUrgencias.Hospital.Models;
using HospitalUrgencias.Hospital.Helpers;

public class PriorityProgram
{
    public static PriorityQueue<Patient, (int Priority, int HospitalArrival)> PriorityPatientQueue =
        new PriorityQueue<Patient, (int Priority, int HospitalArrival)>();
    private static readonly object consultationLock = new object();
    private static readonly List<Thread> DoctorThreads = new List<Thread>();
    public static int processedPatients = 0;
    public static int TotalPatients;
    


    public static void HospitalPriorityProgram(int totalPatients)
    {
        TotalPatients = totalPatients;
        DateTime startSession = DateTime.Now;

        Hospital.CreateDoctors();
        Hospital.CreateCTScanners();
        
        ConsoleView.ShowWelcomeMessage();

        Thread StatManagerThread = new Thread(Statistics.StatsProcess);
        StatManagerThread.Start();

        for (int i = 0; i < Hospital.CTScannerList.Count; i++)
        {
            Thread diagnosticThread = new Thread(Hospital.DiagnosticProcess);
            Hospital.DiagnosticThreads.Add(diagnosticThread);
            diagnosticThread.Start();
        }

        for (int i = 0; i < Hospital.DoctorList.Count; i++)
        {
            Thread doctorConsultationThread = new Thread(ConsultationSelectionProcess);
            DoctorThreads.Add(doctorConsultationThread);
            doctorConsultationThread.Start();
        }

        for (int i = 1; i <= totalPatients; i++)
        {
            int arrivalOrderNum = i;
            Patient patient = new Patient(arrivalOrderNum);

            lock (consultationLock)
            {
                PriorityPatientQueue.Enqueue(patient, ((int)patient.Priority, patient.HospitalArrival));
                ConsoleView.ShowHospitalStatusMessage(patient, showPriorityMessage: true);
                Monitor.Pulse(consultationLock);
            }

            Thread.Sleep(Hospital.patientArrivalInterval);
        }

        lock (consultationLock)
        {
            Monitor.PulseAll(consultationLock);
        }

        foreach (Thread doctor in DoctorThreads) doctor.Join();

        Hospital.DiagnosticQueue.CompleteAdding();

        foreach (Thread diagnostic in Hospital.DiagnosticThreads) diagnostic.Join();

        lock (Statistics.statslock)
        {
            Monitor.PulseAll(Statistics.statslock);
        }

        DateTime endSession = DateTime.Now;
        Statistics.totalSessionTime = endSession - startSession;

        Statistics.CalculateStats();

        ConsoleView.ShowExitMessage();
        ConsoleView.ShowStatMessage();
    }


    public static void ConsultationSelectionProcess()
    {
        while (true)
        {
            Patient? patient = null;

            lock (consultationLock)
            {
                while (PriorityPatientQueue.Count == 0 && processedPatients < TotalPatients)
                {
                    Monitor.Wait(consultationLock);
                }

                if (PriorityPatientQueue.Count == 0 && processedPatients >= TotalPatients) break;

                PriorityPatientQueue.TryDequeue(out patient, out var key);
            }

            if (patient != null)
            {
                if (patient.RequiresDiagnostic)
                {
                    patient.DiagnosticTicket = Hospital.diagnosticTicketTurn.GetTicket();
                    Hospital.DiagnosticQueue.Add(patient);
                }

                PriorityPatientProcess(patient);

                lock (Statistics.statslock)
                {
                    Statistics.statsPatientList.Enqueue(patient);
                    Monitor.Pulse(Statistics.statslock);
                }

                lock (consultationLock)
                {
                    processedPatients++;
                    Monitor.PulseAll(consultationLock);
                }
            }
        }
    }


    public static void PriorityPatientProcess(Patient patient)
    {
        Hospital.consultationSem.Wait();
        patient.PauseWaitingTimer();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        patient.Status = PatientStatus.InConsultation;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor, showPriorityMessage: true);

        Thread.Sleep(patient.ConsultationTime * 1000);
        
        patient.Status = PatientStatus.Finished;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor, showPriorityMessage: true);

        assignedDoctor.ReleaseDoctor();
        Hospital.consultationSem.Release();

        if (patient.RequiresDiagnostic)
        {
            patient.ResumeWaitingTimer();

            lock (Hospital.queueLock)
            {
                Monitor.PulseAll(Hospital.queueLock);
            }
        } else{
            patient.StopWaitingTimer();
        }
    }
}