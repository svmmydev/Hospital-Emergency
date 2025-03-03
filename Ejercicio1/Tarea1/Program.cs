
class Doctor
{
    public int Id {get; set;}
    public bool IsAvailable {get; set;} = true;

    public Doctor(int Id)
    {
        this.Id = Id;
    }
}

internal class Program
{
    static int totalPatients = 4;
    static List<Doctor> doctors = new List<Doctor>
    {
        new Doctor(1),
        new Doctor(2),
        new Doctor(3),
        new Doctor(4)
    };


    static SemaphoreSlim consultSem = new SemaphoreSlim(4);
    static readonly object locker = new object();
    static readonly Random rnd = new Random();


    private static void Main(string[] args)
    {
        Console.WriteLine("Patients are entering the hospital..\n");

        for (int i = 1; i <= totalPatients; i++)
        {
            int id = i;

            Thread pacient = new Thread(() => PatientArrival(id));
            pacient.Start();

            Thread.Sleep(2000);
        }
    }


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


    private static Doctor AssignDoctor()
    {
        Doctor assignedDoctor;

        while (true)
        {
            lock (locker)
            {
                var availableDoctors = doctors.Where(d => d.IsAvailable).ToList(); 

                if (availableDoctors.Count > 0)
                {
                    assignedDoctor = availableDoctors[rnd.Next(availableDoctors.Count)];
                    assignedDoctor.IsAvailable = false;
                    return assignedDoctor;
                }
            }
        }
    }


    private static void ReleaseDoctor(Doctor assignedDoctor, int id)
    {
        lock (locker)
        {
            assignedDoctor.IsAvailable = true;
            Console.WriteLine($"Patient {id} has finished the consultation and the Doctor {assignedDoctor.Id} is now free");
        }
    }
}