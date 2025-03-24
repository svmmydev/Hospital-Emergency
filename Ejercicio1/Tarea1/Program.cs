
using HospitalUrgencias.Models;

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
        ConsoleView.ShowWelcomeMessage();

        // Simulates the arrival of patients at intervals.
        for (int i = 1; i <= Hospital.totalPatients; i++)
        {
            int arrivalOrderNum = i;

            int consultationTime = 10000;

            Thread patient = new Thread(() => PatientProcess(arrivalOrderNum, consultationTime));
            patient.Start();

            Thread.Sleep(Hospital.patientArrivalInterval);
        }
    }


    /// <summary>
    /// Simulates the arrival of a patient and assigns them to a doctor for a consultation.
    /// </summary>
    /// <param name="PatientId">The unique identifier of the arriving patient.</param>
    /// <param name="ConsultationTime">Duration of the consultation.</param>
    private static void PatientProcess(int arrivalOrderNumber, int ConsultationTime)
    {
        Console.WriteLine($"Patient has arrived. Arrival order number: {arrivalOrderNumber}.");

        Hospital.consultationSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        Thread.Sleep(ConsultationTime);

        Console.WriteLine($"Patient with arrival order number: {arrivalOrderNumber} has finished the consultation.");
        
        assignedDoctor.ReleaseDoctor();
        Hospital.consultationSem.Release();
    }

}