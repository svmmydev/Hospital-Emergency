
namespace HospitalUrgencias.Models;

public static class Hospital
{
    public static readonly int totalPatients = 4;
    public static readonly SemaphoreSlim consultSem = new SemaphoreSlim(4);
    public static readonly Random rnd = new Random();
}
