
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


    public static readonly ConcurrentQueue<Patient> PatientQueue = new ConcurrentQueue<Patient>();
    public static readonly BlockingCollection<Patient> DiagnosticQueue = new BlockingCollection<Patient>(PatientQueue);
    public static readonly List<Patient> PatientList = [];
    public static readonly List<Thread> PatientThreads = new List<Thread>();
    public static readonly List<Thread> DiagnosticThreads = [];

    
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
            CTScanner assignedCTScanner = CTScanner.AssignCTScanner();

            patient.Status = PatientStatus.WaitingDiagnostic;
            ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);

            diagnosticTicketTurn.Next();

            Thread.Sleep(medicalTestTime);

            patient.RequiresDiagnostic = false;
            ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);

            assignedCTScanner.ReleaseCTScanner();
            scannerSem.Release();

            lock (queueLock)
            {
                Monitor.PulseAll(queueLock);
            }
        }
    }
}
