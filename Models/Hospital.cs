
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace HospitalUrgencias.Models;

public static class Hospital
{
    public static readonly int totalPatients = 4;
    public static readonly SemaphoreSlim consultSem = new SemaphoreSlim(4);
    public static readonly SemaphoreSlim scannerSem = new SemaphoreSlim(2);
    public static readonly Random rnd = new Random();
    public static readonly object queueLock = new object();


    public static ConcurrentQueue<Patient> PatientQueue = new ConcurrentQueue<Patient>();
    public static BlockingCollection<Patient> DiagnosticQueue = new BlockingCollection<Patient>(PatientQueue);
    private static List<Patient> PatientList = new List<Patient>();
    private static List<Thread> PatientThreads = new List<Thread>();


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
            int Id;
            bool existentId;

            do{
                Id = rnd.Next(1,101);
                existentId = CheckingExistentId(Id);
            }
            while(existentId);

            int consultationTime = rnd.Next(5,16);

            Patient patient = new Patient(Id, arrivalOrderNum, consultationTime);
            PatientList.Add(patient);

            Thread patientThread = new Thread(() => action(patient));
            PatientThreads.Add(patientThread);
            patientThread.Start();

            Thread.Sleep(patientArrivalInterval);
        }

        foreach(Thread thread in PatientThreads)
        {
            thread.Join();
        }

        ConsoleView.ShowExitMessage();
    }


    private static bool CheckingExistentId(int Id)
    {
        foreach(Patient patient in PatientList)
        {
            if(patient.Id == Id) return true;
        }

        return false;
    }
}
