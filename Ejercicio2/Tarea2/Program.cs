
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
        TicketProgram.HospitalTicketProgram(TicketProgram.PatientProcess, 4);
    }
}