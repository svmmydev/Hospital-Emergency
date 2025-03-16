
using hospital_urgencias.Models;

/// <summary>
/// Simulates a concurrent medical consultation system.
/// </summary>
internal class Program
{
    static readonly int totalPatients = 4;
    static readonly List<Doctor> doctors = new List<Doctor>
    {
        new Doctor(1),
        new Doctor(2),
        new Doctor(3),
        new Doctor(4)
    };


    static readonly SemaphoreSlim consultSem = new SemaphoreSlim(4);
    static readonly object locker = new object();
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
            int id = i;

            Thread pacient = new Thread(() => PatientArrival(id));
            pacient.Start();

            Thread.Sleep(2000);
        }
    }


    /// <summary>
    /// Simulates the arrival of a patient and assigns them to a doctor for a consultation.
    /// </summary>
    /// <param name="id">The unique identifier of the arriving patient.</param>
    private static void PatientArrival(int id)
    {
        Console.WriteLine($"Patient has arrived. ID: {id}.");

        consultSem.Wait();
        Doctor assignedDoctor = AssignDoctor();
        Console.WriteLine($"Patient with ID: {id} is actually entering a consult with Doctor {assignedDoctor.Id}.\n");

        Thread.Sleep(10000);

        ReleaseDoctor(assignedDoctor, id);
        consultSem.Release();
    }


    /// <summary>
    /// Selects a random available doctor.
    /// This method blocks execution until a doctor becomes available.
    /// Access to the doctir list is synchronized to ensure data consistency and avoid race conditions.
    /// </summary>
    /// <returns>A Doctor object representing the assigned doctor.</returns>
    private static Doctor AssignDoctor()
    {
        Doctor selectedDoctor;

        while (true)
        {
            lock (locker)
            {
                var availableDoctors = doctors.Where(d => d.IsAvailable).ToList(); 

                if (availableDoctors.Count > 0)
                {
                    selectedDoctor = availableDoctors[rnd.Next(availableDoctors.Count)];
                    selectedDoctor.IsAvailable = false;
                    return selectedDoctor;
                }
            }
        }
    }


    /// <summary>
    /// Marks the doctor as available after a patient finishes their consultation.
    /// </summary>
    /// <param name="assignedDoctor">The doctor who was assigned to the patient.</param>
    /// <param name="patientId">The unique identifier of the patient who has finished the consultation.</param>
    private static void ReleaseDoctor(Doctor assignedDoctor, int patientId)
    {
        lock (locker)
        {
            assignedDoctor.IsAvailable = true;
            Console.WriteLine($"Patient {patientId} has finished the consultation and the Doctor {assignedDoctor.Id} is now free");
        }
    }
}