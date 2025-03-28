
namespace HospitalUrgencias.Hospital.Helpers;

public class TurnTicket
{
    private int nextTicket = 1;
    private int currentTicket = 1;
    private readonly object ticketLock = new object();


    public int GetTicket()
    {
        lock (ticketLock)
        {
            return nextTicket++;
        }
    }


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


    public void Next()
    {
        lock (ticketLock)
        {
            currentTicket++;
            Monitor.PulseAll(ticketLock);
        }
    }
}