
namespace HospitalUrgencias.Hospital.Services;
using HospitalUrgencias.Hospital.Models;
using HospitalUrgencias.Hospital.Helpers;

public class PriorityProgram
{
    public static PriorityQueue<Patient, (int Priority, int HospitalArrival)> PriorityPatientQueue =
        new PriorityQueue<Patient, (int Priority, int HospitalArrival)>();
    private static readonly object consultationLock = new object();
    private static readonly List<Thread> DoctorThreads = new List<Thread>();
    private static int processedPatients = 0;
    private static int TotalPatients;


    public static void HospitalPriorityProgram(int totalPatients)
    {
        TotalPatients = totalPatients;
        ConsoleView.ShowWelcomeMessage();

        for (int i = 0; i < Hospital.CTScannerList.Count; i++)
        {
            Thread diagnosticThread = new Thread(Hospital.DiagnosticProcess);
            Hospital.DiagnosticThreads.Add(diagnosticThread);
            diagnosticThread.Start();
        }

        for (int i = 0; i < Hospital.DoctorList.Count; i++)
        {
            Thread doctorConsultation = new Thread(ConsultationSelectionProcess);
            DoctorThreads.Add(doctorConsultation);
            doctorConsultation.Start();
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

        ConsoleView.ShowExitMessage();
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
                PriorityPatientProcess(patient);

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
        Doctor assignedDoctor = Doctor.AssignDoctor();

        patient.Status = PatientStatus.InConsultation;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor, showPriorityMessage: true);

        if (patient.RequiresDiagnostic)
        {
            patient.DiagnosticTicket = Hospital.diagnosticTicketTurn.GetTicket();
            Hospital.DiagnosticQueue.Add(patient);
        }

        Thread.Sleep(patient.ConsultationTime * 1000);
        
        patient.Status = PatientStatus.Finished;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor, showPriorityMessage: true);

        assignedDoctor.ReleaseDoctor();
        Hospital.consultationSem.Release();

        if (patient.RequiresDiagnostic)
        {
            lock (Hospital.queueLock)
            {
                Monitor.PulseAll(Hospital.queueLock);
            }
        }
    }
}