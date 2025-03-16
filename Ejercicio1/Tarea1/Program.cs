
using HospitalUrgencias.Models;

/// <summary>
/// Simulates a concurrent medical consultation system.
/// </summary>
internal class Program
{
    static readonly int totalPatients = 4;
    static readonly SemaphoreSlim consultSem = new SemaphoreSlim(4);


    /// <summary>
    /// Entry point of the program. Simulates patient arrivals.
    /// </summary>
    private static void Main(string[] args)
    {
        Console.WriteLine("\nPatients are entering the hospital..\n");

        // Simulates the arrival of patients at intervals.
        for (int i = 1; i <= totalPatients; i++)
        {
            int arrivalOrderNumber = i;

            Thread patient = new Thread(() => PatientArrival(arrivalOrderNumber, 10000));
            patient.Start();

            Thread.Sleep(2000);
        }
    }


    /// <summary>
    /// Simulates the arrival of a patient and assigns them to a doctor for a consultation.
    /// </summary>
    /// <param name="PatientId">The unique identifier of the arriving patient.</param>
    /// <param name="ConsultationTime">Duration of the consultation.</param>
    private static void PatientArrival(int arrivalOrderNumber, int ConsultationTime)
    {
        Console.WriteLine($"Patient has arrived. Arrival order number: {arrivalOrderNumber}.");

        consultSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        Thread.Sleep(ConsultationTime);

        Console.WriteLine($"Patient with arrival order number: {arrivalOrderNumber} has finished the consultation.");
        assignedDoctor.ReleaseDoctor();
        consultSem.Release();
    }

}