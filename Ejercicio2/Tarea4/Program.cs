
using HospitalUrgencias.Hospital.Services;


/// <summary>
/// Simulates a concurrent medical consultation system.
/// </summary>
internal class Program
{
    /// <summary>
    /// Entry point of the program. Simulates patient arrivals.
    /// </summary>
    private static void Main(string[] args)
    {
        PriorityProgram.HospitalPriorityProgram(20);
    }
}