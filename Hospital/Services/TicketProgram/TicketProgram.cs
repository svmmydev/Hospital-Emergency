
namespace HospitalUrgencias.Hospital.Services;
using HospitalUrgencias.Hospital.Models;
using HospitalUrgencias.Hospital.Helpers;

/// <summary>
/// Represents the Hospital Program based on ticket order.
/// </summary>
public class TicketProgram
{
    // Common variable
    public static TurnTicket consultationTicketTurn = new TurnTicket();


    /// <summary>
    /// The main method for simulating the hospital ticket system. It initializes the hospital's doctors, CT scanners,
    /// and processes patients using the provided action. Each patient is processed in a separate thread, and diagnostic
    /// threads are also initialized for handling patient diagnostics. The method ensures that all threads are completed
    /// before displaying the exit message.
    /// </summary>
    /// <param name="action">The action to be performed for each patient (e.g., consultation or diagnostic).</param>
    /// <param name="totalPatients">The total number of patients to simulate in the hospital system.</param>
    public static void HospitalTicketProgram(Action<Patient> action, int totalPatients)
    {
        ConsoleView.ShowWelcomeMessage();

        Hospital.CreateDoctors();
        Hospital.CreateCTScanners();

        // Starts a thread for each CT scanner to process diagnostics concurrently
        for (int i = 0; i < Hospital.CTScannerList.Count; i++)
        {
            Thread diagnosticThread = new Thread(Hospital.DiagnosticProcess);
            Hospital.DiagnosticThreads.Add(diagnosticThread);
            diagnosticThread.Start();
        }

        // Simulates the arrival of patients, creating a thread for each patient and performing the given action
        for (int i = 1; i <= totalPatients; i++)
        {
            int arrivalOrderNum = i;

            Patient patient = new Patient(arrivalOrderNum);

            // Starts a new thread for each patient to execute the provided action
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


    /// <summary>
    /// Processes a patient through the consultation process, including waiting for their turn,
    /// consulting with a doctor, and handling diagnostic requirements. It updates the patient's status
    /// and manages synchronization between threads for consultation and diagnostic processing.
    /// </summary>
    /// <param name="patient">The patient to be processed through the consultation and diagnostic process.</param>
    public static void PatientProcess(Patient patient)
    {
        ConsoleView.ShowHospitalStatusMessage(patient);

        consultationTicketTurn.WaitTurn(patient.HospitalArrival);

        Hospital.consultationSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        patient.Status = PatientStatus.InConsultation;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);

        consultationTicketTurn.Next();

        // If the patient requires a diagnostic, assign them a diagnostic ticket and add them to the diagnostic queue
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

        // If the patient requires a diagnostic, pulse all threads to notify that the patient is ready for diagnostic processing
        if (patient.RequiresDiagnostic)
        {
            lock (Hospital.queueLock)
            {
                Monitor.PulseAll(Hospital.queueLock);
            }
        }
    }
}