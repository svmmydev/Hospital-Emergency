
using HospitalUrgencias.Hospital.Models;
using HospitalUrgencias.Hospital.Services;
using HospitalUrgencias.Hospital.Helpers;


/// <summary>
/// Simulates a concurrent medical consultation system.
/// </summary>
internal class Program
{
    /// <summary>
    /// Entry point of the program. Simulates patient arrivals.
    /// </summary>
    private static void Main(string[] args)
    {
        TicketProgram.HospitalTicketProgram(PatientProcess, 4);
    }


    /// <summary>
    /// Processes a patient's consultation by assigning a doctor and, if necessary, initiating the diagnostic process.
    /// </summary>
    /// <param name="patient">The patient to be processed for consultation.</param>
    private static void PatientProcess(Patient patient)
    {
        ConsoleView.ShowHospitalStatusMessage(patient);
        
        Hospital.consultationSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        patient.Status = PatientStatus.InConsultation;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);
        Thread.Sleep(patient.ConsultationTime * 1000);

        if (patient.RequiresDiagnostic) patient.Status = PatientStatus.WaitingDiagnostic;
        else patient.Status = PatientStatus.Finished;

        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);

        assignedDoctor.ReleaseDoctor();
        Hospital.consultationSem.Release();

        if (patient.RequiresDiagnostic) DiagnosticProcess(patient);
    }


    /// <summary>
    /// Handles the diagnostic process for a patient, including assigning a CT scanner,
    /// simulating the diagnostic procedure, and updating the patient's status.
    /// </summary>
    /// <param name="patient">The patient to be processed for diagnostic testing.</param>
    private static void DiagnosticProcess(Patient patient)
    {
        Hospital.scannerSem.Wait();
        CTScanner assignedCTScanner = CTScanner.AssignCTScanner();
        patient.RequiresDiagnostic = false; // Already being treated, helps the console message

        ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);

        Thread.Sleep(Hospital.medicalTestTime);

        patient.DiagnosticCompleted = true;
        patient.Status = PatientStatus.Finished;
        ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);
        
        assignedCTScanner.ReleaseCTScanner();
        Hospital.scannerSem.Release();
    }
}