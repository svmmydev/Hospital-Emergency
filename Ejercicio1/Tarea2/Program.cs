
using HospitalUrgencias.Models;

/// <summary>
/// Simulates a concurrent medical consultation system.
/// </summary>
internal class Program
{
    static readonly int totalPatients = 4;
    static readonly SemaphoreSlim consultSem = new SemaphoreSlim(4);
    static readonly object locker = new object();
    static readonly Random rnd = new Random();


    private static void Main(string[] args)
    {
        Console.WriteLine("Patients are entering the hospital..\n");

        // Simulates the arrival of patients at intervals.
        for (int i = 1; i <= totalPatients; i++)
        {
            int arrivalPriorityNum = i;

            Patient patient = new Patient(rnd.Next(1,101), arrivalPriorityNum, rnd.Next(5,16));

            Thread patient = new Thread(() => PatientArrival(id));
            patient.Start();

            Thread.Sleep(2000);
        }
    }
}