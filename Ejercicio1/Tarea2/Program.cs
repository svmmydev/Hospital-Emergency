
using HospitalUrgencias.Models;

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
        Hospital.HospitalProgram(PatientProcess, 4);
    }


    /// <summary>
    /// Simulates the arrival of a patient and assigns them to a doctor for a consultation.
    /// </summary>
    /// <param name="Patient">The patient with all of his properties.</param>
    private static void PatientProcess(Patient patient)
    {
        Console.WriteLine($"\nPatient has arrived. ID: {patient.Id} and arrival order number: {patient.HospitalArrival}.");

        Hospital.consultationSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        patient.Status = PatientStatus.InConsultation;
        Thread.Sleep(patient.ConsultationTime * 1000);

        patient.Status = PatientStatus.Finished;
        Console.WriteLine($"\nPatient with ID: {patient.Id} and arrival order number: {patient.HospitalArrival} has finished the consultation.");
        
        assignedDoctor.ReleaseDoctor();
        Hospital.consultationSem.Release();
    }
}