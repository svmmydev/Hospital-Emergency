
namespace HospitalUrgencias.Models;

public static class Hospital
{
    public static readonly int totalPatients = 4;
    public static readonly SemaphoreSlim consultSem = new SemaphoreSlim(4);
    public static readonly SemaphoreSlim scannerSem = new SemaphoreSlim(2);
    public static readonly Random rnd = new Random();


    public static readonly int patientArrivalInterval = 2000;
    public static readonly int medicalTestTime = 15000;

    
    // Doctor list
    public static readonly List<Doctor> DoctorList = new List<Doctor>
    {
        new Doctor(1),
        new Doctor(2),
        new Doctor(3),
        new Doctor(4)
    };


    // CTScanner list
    public static readonly List<CTScanner> CTScannerList = new List<CTScanner>
    {
        new CTScanner(1),
        new CTScanner(2)
    };


    public static void HospitalProgram(Action<Patient> action)
    {
        ConsoleView.ShowWelcomeMessage();

        // Simulates the arrival of patients at intervals.
        for (int i = 1; i <= totalPatients; i++)
        {
            int arrivalOrderNum = i;

            int Id = rnd.Next(1,101);
            int consultationTime = rnd.Next(5,16);

            Patient patient = new Patient(Id, arrivalOrderNum, consultationTime);

            Thread patientProccess = new Thread(() => action(patient));
            patientProccess.Start();

            Thread.Sleep(patientArrivalInterval);
        }
    }
}
