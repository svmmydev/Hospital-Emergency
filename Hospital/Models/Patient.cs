
namespace HospitalUrgencias.Hospital.Models;
using HospitalUrgencias.Hospital.Helpers;
using HospitalUrgencias.Hospital.Services;


/// <summary>
/// Enum representing the patient's priority in the hospital.
/// </summary>
public enum PatientPriority
{
    Emergency = 1,
    Urgency = 2,
    GeneralConsultation = 3
}


/// <summary>
/// Enum representing the patient's status in the hospital.
/// </summary>
public enum PatientStatus
{
    WaitingConsultation,
    InConsultation,
    WaitingDiagnostic,
    Finished   
}


/// <summary>
/// Represents a patient in the hospital.
/// </summary>
public class Patient
{
    // Properties
    public int Id {get; private set;}
    public int HospitalArrival {get; private set;}
    public PatientStatus Status {get; set;}
    public int WaitingTime {get; private set;}
    public int ConsultationTime {get; private set;}
    public bool RequiresDiagnostic {get; set;}
    public int DiagnosticTicket {get; set;}
    public int ConsultationTicket {get; private set;}
    public PatientPriority Priority {get; private set;}
    public bool DiagnosticCompleted {get; set;}


    // Common variables
    private Timer? waitingTimer;
    public bool TimerRunning { get; private set; }

    
    /// <summary>
    /// Initializes a new instance of the "Patient" class with a unique ID, a consultation ticket,
    /// the hospital arrival time, status, consultation time, diagnostic requirement, and priority of the patient.
    /// A timer is also started to track the patient's waiting time.
    /// </summary>
    /// <param name="HospitalArrival">The patient's arrival time at the hospital.</param>
    public Patient (int HospitalArrival)
    {
        Id = RandomIdGenerator.GetUniqueId(1000);
        ConsultationTicket = TicketProgram.consultationTicketTurn.GetTicket(); // Optional (Ejercicio 2: Tarea 2 y 3)
        this.HospitalArrival = HospitalArrival;
        Status = PatientStatus.WaitingConsultation;
        ConsultationTime = Hospital.rnd.Next(5,16);
        RequiresDiagnostic = Hospital.rnd.Next(0,2) == 1;
        Priority = (PatientPriority)Hospital.rnd.Next(1,4);
        DiagnosticCompleted = false;
        waitingTimer = new Timer(IncrementWaitingTime, null, 0, 1000);
    }


    /// <summary>
    /// Pauses the waiting timer without liberarlo.
    /// </summary>
    public void PauseWaitingTimer()
    {
        if (waitingTimer != null)
        {
            waitingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            TimerRunning = false;
        }
    }


    /// <summary>
    /// Resumes the waiting timer if it was paused.
    /// </summary>
    public void ResumeWaitingTimer()
    {
        if (waitingTimer != null && !TimerRunning)
        {
            waitingTimer.Change(1000, 1000);
            TimerRunning = true;
        }
    }


    /// <summary>
    /// Stops and disposes the waiting timer.
    /// </summary>
    public void StopWaitingTimer()
    {
        if (waitingTimer != null)
        {
            waitingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            waitingTimer.Dispose();
            waitingTimer = null;
            TimerRunning = false;
        }
    }


    /// <summary>
    /// Increments the patient's waiting time by 1 each time the timer elapses.
    /// This method is called by the timer to track the passage of time while the patient is waiting.
    /// </summary>
    /// <param name="state">An optional state object that is passed by the timer (not used in this implementation).</param>
    private void IncrementWaitingTime(object? state)
    {
        WaitingTime++;
    }
}
