
namespace hospital_urgencias.Models;

/// <summary>
/// Represents a doctor in the hospital.
/// </summary>
public class Doctor
{
    public int Id {get; set;}
    public bool IsAvailable {get; set;} = true;

    /// <summary>
    /// Initializes a new instance of the doctor class.
    /// </summary>
    /// <param name="Id">The unique identification number of the doctor.</param>
    public Doctor(int Id)
    {
        this.Id = Id;
    }
}


