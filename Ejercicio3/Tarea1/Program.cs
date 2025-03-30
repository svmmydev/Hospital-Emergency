
using HospitalUrgencias.Hospital.Helpers;
using HospitalUrgencias.Hospital.Models;
using HospitalUrgencias.Hospital.Services;


/// <summary>
/// Simulates a concurrent medical consultation system.
/// </summary>
internal class Program
{
    //----------------------------------------------------//
    //               EJERCICIO 2: TAREA 1                 //   [100 PATIENTS]
    //----------------------------------------------------//


    /// <summary>
    /// Entry point of the program. Simulates patient arrivals.
    /// </summary>
    private static void Main(string[] args)
    {
        TicketProgram.HospitalTicketProgram(PatientProcess, 100);
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

        patient.Status = PatientStatus.Finished;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);

        assignedDoctor.ReleaseDoctor();
        Hospital.consultationSem.Release();

        if (patient.RequiresDiagnostic)
        {
            DiagnosticProcess(patient);
        }
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

        patient.Status = PatientStatus.WaitingDiagnostic;
        ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);

        Thread.Sleep(Hospital.medicalTestTime);

        patient.RequiresDiagnostic = false;
        ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);
        
        assignedCTScanner.ReleaseCTScanner();
        Hospital.scannerSem.Release();
    }


    //----------------------------------------------------//
    //               EJERCICIO 2: TAREA 2                 //   [200 PATIENTS]
    //----------------------------------------------------//

    /*
    /// <summary>
    /// Entry point of the program. Simulates patient arrivals.
    /// </summary>
    private static void Main(string[] args)
    {
        TicketProgram.HospitalTicketProgram(TicketProgram.PatientProcess, 200);
    }
    */


    //----------------------------------------------------//
    //               EJERCICIO 2: TAREA 3                 //   [500 PATIENTS]
    //----------------------------------------------------//

    /*
    /// <summary>
    /// Entry point of the program. Simulates patient arrivals.
    /// </summary>
    private static void Main(string[] args)
    {
        TicketProgram.HospitalTicketProgram(TicketProgram.PatientProcess, 500);
    }
    */
}