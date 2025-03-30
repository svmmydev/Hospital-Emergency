
namespace HospitalUrgencias.Hospital.Services;
using HospitalUrgencias.Hospital.Models;
using HospitalUrgencias.Hospital.Helpers;
using System.Collections.Concurrent;


/// <summary>
/// Represents the common logic and the properties of the Hospital
/// </summary>
public static class Hospital
{
    // Common variables
    public static readonly SemaphoreSlim consultationSem = new SemaphoreSlim(4);
    public static TurnTicket diagnosticTicketTurn = new TurnTicket();
    public static readonly SemaphoreSlim scannerSem = new SemaphoreSlim(2);
    public static readonly Random rnd = new Random();
    public static readonly object queueLock = new object();

    // Const common variables
    public const int patientArrivalInterval = 2000;
    public const int medicalTestTime = 15000;
    private const int numberOfDoctors = 4;
    private const int numberOfCTScanners = 2;

    // Common lists
    public static readonly ConcurrentQueue<Patient> PatientQueue = new ConcurrentQueue<Patient>();
    public static readonly BlockingCollection<Patient> DiagnosticQueue = new BlockingCollection<Patient>(PatientQueue);
    public static readonly List<Thread> PatientThreads = new List<Thread>();
    public static readonly List<Thread> DiagnosticThreads = [];

    
    // Doctor and CTScanner lists
    public static readonly List<Doctor> DoctorList = new List<Doctor>();
    public static readonly List<CTScanner> CTScannerList = new List<CTScanner>();


    /// <summary>
    /// Processes the diagnostic procedure for patients who require it. This includes waiting for the patient's status
    /// to be marked as "Finished", assigning a CT scanner, performing the diagnostic test, and updating the patient's status
    /// and statistics accordingly. The method also manages synchronization to ensure the correct sequence of actions
    /// and resource allocation (CT scanner and patient timers).
    /// </summary>
    public static void DiagnosticProcess()
    {
        // Continuously processes patients from the diagnostic queue until it's empty
        foreach (var patient in DiagnosticQueue.GetConsumingEnumerable())
        {
            diagnosticTicketTurn.WaitTurn(patient.DiagnosticTicket);

            // Lock the queue to safely check the patient's status before proceeding with diagnostics
            lock (queueLock)
            {
                // Wait until the patient's status is "Finished" before proceeding with diagnostics
                while (patient.Status != PatientStatus.Finished)
                {
                    Monitor.Wait(queueLock); // Thread waiting spot
                }
            }

            scannerSem.Wait();
            patient.PauseWaitingTimer();
            
            CTScanner assignedCTScanner = CTScanner.AssignCTScanner();

            patient.Status = PatientStatus.WaitingDiagnostic;
            ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);

            diagnosticTicketTurn.Next();

            // Calculates the usage time of the scanner by timing marks
            DateTime startUsage = DateTime.Now; // Get timing mark
            Thread.Sleep(medicalTestTime);

            // Calculates the timing mark difference
            double scannerUsageTime = (DateTime.Now - startUsage).TotalSeconds;
            Statistics.DiagnosticUsage(scannerUsageTime);

            patient.RequiresDiagnostic = false;
            ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);

            assignedCTScanner.ReleaseCTScanner();
            scannerSem.Release();
            patient.StopWaitingTimer();

            // Notify all other threads that the patient has finished the diagnostic process
            lock (queueLock)
            {
                Monitor.PulseAll(queueLock);
            }
        }
    }


    /// <summary>
    /// Creates a specified number of doctors and adds them to the list of doctors in the hospital.
    /// Each doctor is assigned a unique ID starting from 1 to the specified number of doctors.
    /// The number of Doctors to add is defined in the class as a property.
    /// </summary>
    public static void CreateDoctors()
    {
        for (int i = 1; i <= numberOfDoctors; i++)
        {
            Doctor doctor = new Doctor(i);
            DoctorList.Add(doctor);
        }
    }
    

    /// <summary>
    /// Creates a specified number of CT scanners and adds them to the list of CT scanners in the hospital.
    /// Each scanner is assigned a unique ID starting from 1 to the specified number of CT scanners.
    /// </summary>
    public static void CreateCTScanners()
    {
        for (int i = 1; i <= numberOfCTScanners; i++)
        {
            CTScanner scanner = new CTScanner(i);
            CTScannerList.Add(scanner);
        }
    }
}
