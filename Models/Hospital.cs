
namespace HospitalUrgencias.Models;

public static class Hospital
{
    public static readonly int totalPatients = 4;
    public static readonly SemaphoreSlim consultSem = new SemaphoreSlim(4);
    public static readonly Random rnd = new Random();

    
    // Doctor list
    public static readonly List<Doctor> DoctorList = new List<Doctor>
    {
        new Doctor(1),
        new Doctor(2),
        new Doctor(3),
        new Doctor(4)
    };


    
}
