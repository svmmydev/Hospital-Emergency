
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
        Console.WriteLine("\nPatients are entering the hospital..\n");

        // Simulates the arrival of patients at intervals.
        for (int i = 1; i <= Hospital.totalPatients; i++)
        {
            int arrivalOrderNum = i;

            Patient patient = new Patient(Hospital.rnd.Next(1,101), arrivalOrderNum, Hospital.rnd.Next(5,16));

            Thread patientProccess = new Thread(() => PatientArrival(patient));
            patientProccess.Start();

            Thread.Sleep(2000);
        }
    }


    /// <summary>
    /// Simulates the arrival of a patient and assigns them to a doctor for a consultation.
    /// </summary>
    /// <param name="Patient">The patient with all of his properties.</param>
    private static void PatientArrival(Patient patient)
    {
        Hospital.consultSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        string statusMsg = patient.UpdatingPatientStatus(PatientStatus.InConsultation, assignedDoctor);
        Console.WriteLine(statusMsg);
        Thread.Sleep(patient.ConsultationTime * 1000);

        statusMsg = patient.UpdatingPatientStatus(PatientStatus.Finished, assignedDoctor);
        Console.WriteLine(statusMsg);
        assignedDoctor.ReleaseDoctor();
        Hospital.consultSem.Release();
    }
}