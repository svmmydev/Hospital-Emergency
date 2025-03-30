
namespace HospitalUrgencias.Hospital.Helpers;


/// <summary>
/// Represents a turn ticket machine (queue).
/// </summary>
public class TurnTicket
{
    // Common variables
    private int nextTicket = 1;
    private int currentTicket = 1;
    private readonly object ticketLock = new object();


    // <summary>
    /// Retrieves the next available ticket number for a patient.
    /// </summary>
    /// <returns>The next available ticket number.</returns>
    public int GetTicket()
    {
        lock (ticketLock)
        {
            return nextTicket++;
        }
    }


    /// <summary>
    /// Blocks the current thread until the patient's ticket number matches the current ticket.
    /// </summary>
    /// <param name="patientTicket">The ticket number of the patient waiting for their turn.</param>
    public void WaitTurn(int patientTicket)
    {
        lock (ticketLock)
        {
            while (patientTicket != currentTicket)
            {
                Monitor.Wait(ticketLock);
            }
        }
    }


    /// <summary>
    /// Increments the current ticket number and notifies all waiting threads.
    /// </summary>
    public void Next()
    {
        lock (ticketLock)
        {
            currentTicket++;
            Monitor.PulseAll(ticketLock);
        }
    }
}