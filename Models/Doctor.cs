
namespace HospitalUrgencias.Models;

/// <summary>
/// Represents a doctor in the hospital.
/// </summary>
public class Doctor
{
    // Properties
    public int Id {get; private set;}
    public string ReferenceName {get; private set;}
    public bool IsAvailable {get; set;} = true;


    // Common variables
    static readonly object locker = new object();


    /// <summary>
    /// Initializes a new instance of the doctor class.
    /// </summary>
    /// <param name="Id">The unique identification number of the doctor.</param>
    public Doctor(int Id)
    {
        this.Id = Id;
        ReferenceName = $"Doctor {Id}";
    }


    /// <summary>
    /// Selects a random available doctor.
    /// This method blocks execution until a doctor becomes available.
    /// Access to the doctor list is synchronized to ensure data consistency and avoid race conditions.
    /// </summary>
    /// <returns>A Doctor object representing the assigned doctor.</returns>
    public static Doctor AssignDoctor()
    {
        Doctor selectedDoctor;

        while (true)
        {
            lock (locker)
            {
                var availableDoctors = Hospital.DoctorList.Where(d => d.IsAvailable).ToList(); 

                if (availableDoctors.Count > 0)
                {
                    selectedDoctor = availableDoctors[Hospital.rnd.Next(availableDoctors.Count)];
                    selectedDoctor.IsAvailable = false;
                    return selectedDoctor;
                }
            }
        }
    }


    /// <summary>
    /// Marks the doctor as available after a patient finishes the consultation.
    /// </summary>
    public void ReleaseDoctor()
    {
        lock (locker)
        {
            IsAvailable = true;
        }
    }
}


