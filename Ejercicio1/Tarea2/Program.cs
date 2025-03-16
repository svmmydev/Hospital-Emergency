
using hospital_urgencias.Models;


internal class Program
{
    static readonly int totalPatients = 4;
    static readonly SemaphoreSlim consultSem = new SemaphoreSlim(4);
    static readonly object locker = new object();
    static readonly Random rnd = new Random();


    private static void Main(string[] args)
    {
        Console.WriteLine("Patients are entering the hospital..\n");
        
    }
}