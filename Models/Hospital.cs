
using System.Collections.Concurrent;

namespace HospitalUrgencias.Models;

//TODO: Ordenar todo el código según semántica
public static class Hospital
{
    public const int totalPatients = 4;
    public static readonly SemaphoreSlim consultationSem = new SemaphoreSlim(4);
    public static readonly SemaphoreSlim scannerSem = new SemaphoreSlim(2);
    public static readonly Random rnd = new Random();
    public static readonly object queueLock = new object();


    public const int patientArrivalInterval = 2000;
    public const int medicalTestTime = 15000;
    private static bool AllPatientsFinished = false;


    public static ConcurrentQueue<Patient> PatientQueue = new ConcurrentQueue<Patient>();
    public static BlockingCollection<Patient> DiagnosticQueue = new BlockingCollection<Patient>(PatientQueue);
    private static List<Patient> PatientList = new List<Patient>();
    private static List<Thread> PatientThreads = new List<Thread>();
    private static List<Thread> DiagnosticThreads = new List<Thread>();

    
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


    public static void HospitalProgram(Action<Patient> action, int customTotalPatients = totalPatients)
    {
        ConsoleView.ShowWelcomeMessage();

        for (int i = 0; i < CTScannerList.Count; i++)
        {
            Thread diagnosticThread = new Thread(DiagnosticProcess);
            DiagnosticThreads.Add(diagnosticThread);
            diagnosticThread.Start();
        }

        // Simulates the arrival of patients at intervals.
        for (int i = 1; i <= customTotalPatients; i++)
        {
            int arrivalOrderNum = i;
            int Id;
            bool existentId;

            do{
                Id = rnd.Next(1,101);
                existentId = CheckingExistentId(Id);
            } while(existentId);

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

        //TODO: AÑADIR CONSOLA TODAS LAS CONSULTAS HAN TERMINADO

        DiagnosticQueue.CompleteAdding();

        foreach(Thread thread in DiagnosticThreads)
        {
            thread.Join();
        }

        //TODO: AÑADIR CONSOLA TODOS LOS DIAGNOSTICOS HAN TERMINADO

        ConsoleView.ShowExitMessage();
    }


    private static void DiagnosticProcess()
    {
        foreach (var patient in DiagnosticQueue.GetConsumingEnumerable())
        {
            lock (queueLock)
            {
                while (patient.Status != PatientStatus.Finished)
                {
                    Monitor.Wait(queueLock);
                }
            }

            CTScanner assignedCTScanner = CTScanner.AssignCTScanner();
            scannerSem.Wait();

            patient.Status = PatientStatus.WaitingDiagnostic;
            ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);
            Thread.Sleep(medicalTestTime);

            patient.RequiresDiagnostic = false;
            ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);

            assignedCTScanner.ReleaseCTScanner();
            scannerSem.Release();
        }
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
