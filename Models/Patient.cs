
namespace HospitalUrgencias.Models;


/// <summary>
/// Enum representing the patient's status in the hospital.
/// </summary>
public enum PatientStatus
{
    Waiting,
    InConsultation,
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
    public int ConsultationTime {get; private set;}
    public PatientStatus Status {get; set;} = PatientStatus.Waiting;
    public int WaitingTime {get; private set;}


    // Common variables
    private Timer? waitingTimer;
    public bool TimerRunning { get; private set; }

    
    /// <summary>
    /// Initializes a new instance of the patient class.
    /// </summary>
    /// <param name="Id">The unique identification number of the doctor.</param>
    /// <param name="HospitalArrival">The patient's waiting time.</param>
    /// <param name="ConsultationTime">The consultation time the patient needs.</param>
    /// <param name="startTimer">The patient's waiting timer.</param>
    public Patient (int Id, int HospitalArrival, int ConsultationTime, bool startTimer = false)
    {
        this.Id = Id;
        this.HospitalArrival = HospitalArrival;
        this.ConsultationTime = ConsultationTime;
        
        if (startTimer)
        {
            waitingTimer = new Timer(IncrementWaitingTime, null, 0, 1000);
        }
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


    private void IncrementWaitingTime(object? state)
    {
        WaitingTime++;
    }


    public void ChangingPatientStatus(PatientStatus patientStatus, Doctor assignedDoctor)
    {
        Status = patientStatus;

        string statusMsg;

        switch (Status)
        {
            case PatientStatus.InConsultation:
                statusMsg = $"| Assigned Doctor: {assignedDoctor.ReferenceName} " +
                            $"| Waiting duration: {WaitingTime}";
                break;
            case PatientStatus.Finished:
                statusMsg = $"({assignedDoctor.ReferenceName} is now free) " +
                            $"| Consultation duration: {ConsultationTime}" ;
                break;
            default:
                statusMsg = "";
                break;
        }

        Console.WriteLine(
            $"| Patient {Id} " +
            $"| Arrived as {HospitalArrival} " +
            $"| Status: {patientStatus} " +
            $"{statusMsg} |"
        );
    }
}
