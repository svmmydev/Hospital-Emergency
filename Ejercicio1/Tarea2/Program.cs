
using HospitalUrgencias.Models;

/// <summary>
/// Simulates a concurrent medical consultation system.
/// </summary>
internal class Program
{
    static readonly int totalPatients = 4;
    static readonly SemaphoreSlim consultSem = new SemaphoreSlim(4);
    static readonly Random rnd = new Random();


    /// <summary>
    /// Entry point of the program. Simulates patient arrivals.
    /// </summary>
    private static void Main(string[] args)
    {
        Console.WriteLine("Patients are entering the hospital..\n");

        // Simulates the arrival of patients at intervals.
        for (int i = 1; i <= totalPatients; i++)
        {
            int arrivalPriorityNum = i;

            Patient patient = new Patient(rnd.Next(1,101), arrivalPriorityNum, rnd.Next(5,16));

            Thread patientProccess = new Thread(() => PatientArrival(patient));
            patientProccess.Start();

            Thread.Sleep(2000);
        }
    }


    /// <summary>
    /// Simulates the arrival of a patient and assigns them to a doctor for a consultation.
    /// </summary>
    /// <param name="Patient">The patient with all of his properties.</param>
    private static void PatientArrival(Patient patient)
    {
        Console.WriteLine($"Patient has arrived. ID: {patient.Id} and Priority: {patient.HospitalArrivalPriority}.");

        consultSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();
        Console.WriteLine($"Patient with ID: {patient.Id} is actually entering a consult with Doctor {assignedDoctor.Id}.\n");

        patient.Status = PatientStatus.InConsultation;
        Thread.Sleep(patient.ConsultationTime * 1000);

        patient.Status = PatientStatus.Finished;
        Doctor.ReleaseDoctor(assignedDoctor, patient.Id);
        consultSem.Release();
    }
}