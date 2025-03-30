
namespace HospitalUrgencias.Hospital.Services;
using HospitalUrgencias.Hospital.Models;
using HospitalUrgencias.Hospital.Helpers;

/// <summary>
/// Represents the Hospital Program based on priority levels.
/// </summary>
public class PriorityProgram
{
    // Common variables
    public static PriorityQueue<Patient, (int Priority, int HospitalArrival)> PriorityPatientQueue =
        new PriorityQueue<Patient, (int Priority, int HospitalArrival)>();
    private static readonly object consultationLock = new object();
    private static readonly List<Thread> DoctorThreads = new List<Thread>();
    public static int processedPatients = 0;
    public static int TotalPatients;
    

    /// <summary>
    /// The main method that simulates the hospital process, managing patient arrivals, consultations, and diagnostics.
    /// It initializes threads for managing doctors, diagnostic scanners, and patient processing, while calculating and
    /// displaying various statistics at the end of the session.
    /// </summary>
    /// <param name="totalPatients">The total number of patients to simulate in the hospital system.</param>
    public static void HospitalPriorityProgram(int totalPatients)
    {
        // Sets the total number of patients for the session
        TotalPatients = totalPatients;

        DateTime startSession = DateTime.Now;

        Hospital.CreateDoctors();
        Hospital.CreateCTScanners();
        
        ConsoleView.ShowWelcomeMessage();

        // Starts a thread to manage statistics during the session
        Thread StatManagerThread = new Thread(Statistics.StatsProcess);
        StatManagerThread.Start();

        // Starts diagnostic threads for each CT scanner
        for (int i = 0; i < Hospital.CTScannerList.Count; i++)
        {
            Thread diagnosticThread = new Thread(Hospital.DiagnosticProcess);
            Hospital.DiagnosticThreads.Add(diagnosticThread);
            diagnosticThread.Start();
        }

        // Starts diagnostic threads for each CT scanner
        for (int i = 0; i < Hospital.DoctorList.Count; i++)
        {
            Thread doctorConsultationThread = new Thread(ConsultationSelectionProcess);
            DoctorThreads.Add(doctorConsultationThread);
            doctorConsultationThread.Start();
        }

        // Simulates de arrival of patients
        for (int i = 1; i <= totalPatients; i++)
        {
            int arrivalOrderNum = i;
            Patient patient = new Patient(arrivalOrderNum);

            // Locks the consultation queue to add the patient in priority order
            lock (consultationLock)
            {
                PriorityPatientQueue.Enqueue(patient, ((int)patient.Priority, patient.HospitalArrival));
                ConsoleView.ShowHospitalStatusMessage(patient, showPriorityMessage: true);
                Monitor.Pulse(consultationLock);
            }

            Thread.Sleep(Hospital.patientArrivalInterval);
        }

        // Signals the threads to finish processing remaining patients (waking up them)
        lock (consultationLock)
        {
            Monitor.PulseAll(consultationLock);
        }

        foreach (Thread doctor in DoctorThreads) doctor.Join();

        Hospital.DiagnosticQueue.CompleteAdding();
        foreach (Thread diagnostic in Hospital.DiagnosticThreads) diagnostic.Join();

        // Signals the statistics thread to finish processing
        lock (Statistics.statslock)
        {
            Monitor.PulseAll(Statistics.statslock);
        }

        // Marks the end of the session and calculates total session time
        DateTime endSession = DateTime.Now;
        Statistics.totalSessionTime = endSession - startSession;

        Statistics.CalculateStats();

        ConsoleView.ShowExitMessage();
        ConsoleView.ShowStatMessage();
    }


    /// <summary>
    /// Continuously processes patients from the priority queue for consultations.
    /// It handles diagnostic ticket assignment, processes the consultation, and updates statistics after processing each patient.
    /// </summary>
    public static void ConsultationSelectionProcess()
    {
        while (true)
        {
            Patient? patient = null;

            // Lock the consultation queue to safely access and dequeue patients
            lock (consultationLock)
            {
                // Wait for patients to be available in the queue or until all patients have been processed
                while (PriorityPatientQueue.Count == 0 && processedPatients < TotalPatients)
                {
                    Monitor.Wait(consultationLock); // Thread waiting spot
                }

                // Exit the loop if there are no more patients and all have been processed
                if (PriorityPatientQueue.Count == 0 && processedPatients >= TotalPatients) break;

                // Dequeue the next patient based on priority
                PriorityPatientQueue.TryDequeue(out patient, out var key);
            }

            if (patient != null)
            {
                // If the patient requires a diagnostic, assign a diagnostic ticket and add them to the diagnostic queue
                if (patient.RequiresDiagnostic)
                {
                    patient.DiagnosticTicket = Hospital.diagnosticTicketTurn.GetTicket();
                    Hospital.DiagnosticQueue.Add(patient);
                }

                PriorityPatientProcess(patient);

                // Lock the statistics section to safely update patient statistics
                lock (Statistics.statslock)
                {
                    Statistics.statsPatientList.Enqueue(patient);
                    Monitor.Pulse(Statistics.statslock); // Notify the statistics thread that the patient has been processed
                }

                // Lock the consultation queue and increment the count of processed patients
                lock (consultationLock)
                {
                    processedPatients++;
                    Monitor.PulseAll(consultationLock); // Notify other threads that a patient has been processed
                }
            }
        }
    }


    /// <summary>
    /// Processes a patient's consultation by assigning a doctor, updating the patient's status, and managing the consultation time.
    /// It handles synchronization using a semaphore to ensure that only one consultation happens at a time.
    /// If the patient requires a diagnostic, it resumes their waiting timer and notifies other threads.
    /// </summary>
    /// <param name="patient">The patient being processed for consultation.</param>
    public static void PriorityPatientProcess(Patient patient)
    {
        Hospital.consultationSem.Wait();
        patient.PauseWaitingTimer();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        patient.Status = PatientStatus.InConsultation;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor, showPriorityMessage: true);

        Thread.Sleep(patient.ConsultationTime * 1000);
        
        if (patient.RequiresDiagnostic) patient.Status = PatientStatus.WaitingDiagnostic;
        else patient.Status = PatientStatus.Finished;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor, showPriorityMessage: true);

        assignedDoctor.ReleaseDoctor();
        Hospital.consultationSem.Release();

        if (patient.RequiresDiagnostic)
        {
            patient.ResumeWaitingTimer();
            
            // Notify all threads that a patient has finished their consultation and may need further processing (diagnostic)
            lock (Hospital.queueLock)
            {
                Monitor.PulseAll(Hospital.queueLock);
            }
        } 
        else
        {
            patient.StopWaitingTimer();
        }
    }
}