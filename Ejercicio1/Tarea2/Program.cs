
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
        Hospital.HospitalProgram(PatientProcess);
    }


    /// <summary>
    /// Simulates the arrival of a patient and assigns them to a doctor for a consultation.
    /// </summary>
    /// <param name="Patient">The patient with all of his properties.</param>
    private static void PatientProcess(Patient patient)
    {
        Console.WriteLine($"Patient has arrived. ID: {patient.Id} and arrival order number: {patient.HospitalArrival}.");

        Hospital.consultSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        patient.Status = PatientStatus.InConsultation;
        Thread.Sleep(patient.ConsultationTime * 1000);

        patient.Status = PatientStatus.Finished;
        Console.WriteLine($"Patient with ID: {patient.Id} and arrival order number: {patient.HospitalArrival} has finished the consultation.");
        assignedDoctor.ReleaseDoctor();
        Hospital.consultSem.Release();
    }
}