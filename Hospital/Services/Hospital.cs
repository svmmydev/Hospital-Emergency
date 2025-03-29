
namespace HospitalUrgencias.Hospital.Services;
using HospitalUrgencias.Hospital.Models;
using HospitalUrgencias.Hospital.Helpers;
using System.Collections.Concurrent;

public static class Hospital
{
    public static readonly SemaphoreSlim consultationSem = new SemaphoreSlim(4);
    public static TurnTicket diagnosticTicketTurn = new TurnTicket();
    public static readonly SemaphoreSlim scannerSem = new SemaphoreSlim(2);
    public static readonly Random rnd = new Random();
    public static readonly object queueLock = new object();


    public const int patientArrivalInterval = 2000;
    public const int medicalTestTime = 15000;
    private const int numberOfDoctors = 4;
    private const int numberOfCTScanners = 2;


    public static readonly ConcurrentQueue<Patient> PatientQueue = new ConcurrentQueue<Patient>();
    public static readonly BlockingCollection<Patient> DiagnosticQueue = new BlockingCollection<Patient>(PatientQueue);
    public static readonly List<Thread> PatientThreads = new List<Thread>();
    public static readonly List<Thread> DiagnosticThreads = [];

    
    // Doctor and CTScanner lists
    public static readonly List<Doctor> DoctorList = new List<Doctor>();
    public static readonly List<CTScanner> CTScannerList = new List<CTScanner>();


    public static void DiagnosticProcess()
    {
        foreach (var patient in DiagnosticQueue.GetConsumingEnumerable())
        {
            diagnosticTicketTurn.WaitTurn(patient.DiagnosticTicket);

            lock (queueLock)
            {
                while (patient.Status != PatientStatus.Finished)
                {
                    Monitor.Wait(queueLock);
                }
            }

            scannerSem.Wait();
            patient.PauseWaitingTimer();
            CTScanner assignedCTScanner = CTScanner.AssignCTScanner();

            patient.Status = PatientStatus.WaitingDiagnostic;
            ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);

            diagnosticTicketTurn.Next();

            DateTime startUsage = DateTime.Now;
            Thread.Sleep(medicalTestTime);
            double scannerUsageTime = (DateTime.Now - startUsage).TotalSeconds;
            Statistics.DiagnosticUsage(scannerUsageTime);

            patient.RequiresDiagnostic = false;
            ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);

            assignedCTScanner.ReleaseCTScanner();
            scannerSem.Release();
            patient.StopWaitingTimer();

            lock (queueLock)
            {
                Monitor.PulseAll(queueLock);
            }
        }
    }


    public static void CreateDoctors()
    {
        for (int i = 0; i < numberOfDoctors; i++)
        {
            Doctor doctor = new Doctor(i);
            DoctorList.Add(doctor);
        }
    }
    
    public static void CreateCTScanners()
    {
        for (int i = 0; i < numberOfCTScanners; i++)
        {
            CTScanner scanner = new CTScanner(i);
            CTScannerList.Add(scanner);
        }
    }
}
