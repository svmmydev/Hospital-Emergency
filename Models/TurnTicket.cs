
namespace HospitalUrgencias.Models;

public static class TurnTicket
{
    private static int nextTicket = 1;
    private static int currentTicket = 1;
    private static readonly object ticketLock = new object();


    public static int GetTicket()
    {
        lock (ticketLock)
        {
            return nextTicket++;
        }
    }


    public static void WaitTurn(int patientTicket)
    {
        lock (ticketLock)
        {
            while (patientTicket != currentTicket)
            {
                Monitor.Wait(ticketLock);
            }
        }
    }


    public static void Next()
    {
        lock (ticketLock)
        {
            currentTicket++;
            Monitor.PulseAll(ticketLock);
        }
    }
}