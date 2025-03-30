
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
    /// Simulates the arrival of a patient and assigns them to a doctor for a consultation.
    /// </summary>
    /// <param name="Patient">The patient to be processed for consultation.</param>
    private static void PatientProcess(Patient patient)
    {
        Hospital.consultationSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        patient.Status = PatientStatus.InConsultation;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);
        Thread.Sleep(patient.ConsultationTime * 1000);

        patient.Status = PatientStatus.Finished;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);
        
        assignedDoctor.ReleaseDoctor();
        Hospital.consultationSem.Release();
    }
}