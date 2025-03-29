
namespace HospitalUrgencias.Hospital.Services;
using HospitalUrgencias.Hospital.Models;
using HospitalUrgencias.Hospital.Helpers;

public class TicketProgram
{
    public static TurnTicket consultationTicketTurn = new TurnTicket();


    public static void HospitalTicketProgram(Action<Patient> action, int totalPatients)
    {
        ConsoleView.ShowWelcomeMessage();

        Hospital.CreateDoctors();
        Hospital.CreateCTScanners();

        for (int i = 0; i < Hospital.CTScannerList.Count; i++)
        {
            Thread diagnosticThread = new Thread(Hospital.DiagnosticProcess);
            Hospital.DiagnosticThreads.Add(diagnosticThread);
            diagnosticThread.Start();
        }

        for (int i = 1; i <= totalPatients; i++)
        {
            int arrivalOrderNum = i;

            Patient patient = new Patient(arrivalOrderNum);

            Thread patientThread = new Thread(() => action(patient));
            Hospital.PatientThreads.Add(patientThread);
            patientThread.Start();

            Thread.Sleep(Hospital.patientArrivalInterval);
        }

        foreach(Thread thread in Hospital.PatientThreads) thread.Join();

        Hospital.DiagnosticQueue.CompleteAdding();

        foreach(Thread thread in Hospital.DiagnosticThreads) thread.Join();

        ConsoleView.ShowExitMessage();
    }


    public static void PatientProcess(Patient patient)
    {
        ConsoleView.ShowHospitalStatusMessage(patient);

        consultationTicketTurn.WaitTurn(patient.HospitalArrival);

        Hospital.consultationSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        patient.Status = PatientStatus.InConsultation;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);

        consultationTicketTurn.Next();

        if (patient.RequiresDiagnostic)
        {
            patient.DiagnosticTicket = Hospital.diagnosticTicketTurn.GetTicket();
            Hospital.DiagnosticQueue.Add(patient);
        }

        Thread.Sleep(patient.ConsultationTime * 1000);
        
        patient.Status = PatientStatus.Finished;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);

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