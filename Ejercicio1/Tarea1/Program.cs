﻿
using HospitalUrgencias.Hospital.Models;
using HospitalUrgencias.Hospital.Services;
using HospitalUrgencias.Hospital.Helpers;


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

        Hospital.CreateDoctors();

        // Simulates the arrival of patients at intervals.
        for (int i = 1; i <= 4; i++)
        {
            int arrivalOrderNum = i;

            int consultationTime = 10000;

            Thread patient = new Thread(() => PatientProcess(arrivalOrderNum, consultationTime));
            Hospital.PatientThreads.Add(patient);
            patient.Start();

            Thread.Sleep(Hospital.patientArrivalInterval);
        }

        foreach (Thread thread in Hospital.PatientThreads) thread.Join();

        ConsoleView.ShowExitMessage();
    }


    /// <summary>
    /// Simulates the arrival of a patient and assigns them to a doctor for a consultation.
    /// </summary>
    /// <param name="arrivalOrderNumber">The unique identifier of the arriving patient.</param>
    /// <param name="ConsultationTime">Duration of the consultation.</param>
    private static void PatientProcess(int arrivalOrderNumber, int ConsultationTime)
    {
        Console.WriteLine($"\nPatient has arrived. Arrival order number: {arrivalOrderNumber}.");

        Hospital.consultationSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        Thread.Sleep(ConsultationTime);

        Console.WriteLine($"\nPatient with arrival order number: {arrivalOrderNumber} has finished the consultation.");
        
        assignedDoctor.ReleaseDoctor();
        Hospital.consultationSem.Release();
    }
}