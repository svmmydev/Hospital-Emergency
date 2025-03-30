
namespace HospitalUrgencias.Hospital.Services;
using HospitalUrgencias.Hospital.Models;


/// <summary>
/// Represents the statistics of the simulation.
/// </summary>
public class Statistics
{
    // Total and Average common variables for Patients
    public static int totalEmergencyPatients, totalUrgencyPatients, totalGeneralPatients;
    public static int totalEmergencyWaitingTime, totalUrgencyWaitingTime, totalGeneralWaitingTime;
    public static double averageEmergencyWaitingTime, averageUrgencyWaitingTime, averageGeneralWaitingTime;

    // Total and Average common variables for CT Scanners
    public static double totalScannerUsageTime, averageScannerUsedTime;

    // Common variables
    public static Queue<Patient> statsPatientList = new Queue<Patient>();
    public static object statslock = new object();
    public static TimeSpan totalSessionTime;


    /// <summary>
    /// Calculates and updates the average waiting times for emergency, urgency, and general patients,
    /// as well as the average scanner usage time as a percentage of the total session time.
    /// </summary>
    public static void CalculateStats()
    {
        averageEmergencyWaitingTime = totalEmergencyPatients > 0 ? totalEmergencyWaitingTime / totalEmergencyPatients : 0;
        averageUrgencyWaitingTime = totalUrgencyPatients > 0 ? totalUrgencyWaitingTime / totalUrgencyPatients : 0;
        averageGeneralWaitingTime = totalGeneralPatients > 0 ? totalGeneralWaitingTime / totalGeneralPatients : 0;

        averageScannerUsedTime = totalSessionTime.TotalSeconds > 0
        ? (int)(totalScannerUsageTime / (Hospital.CTScannerList.Count * totalSessionTime.TotalSeconds) * 100)
        : 0;
    }


    /// <summary>
    /// Adds the given time in seconds to the total scanner usage time, representing the amount of time a CT scanner has been used.
    /// </summary>
    /// <param name="seconds">The amount of time (in seconds) the CT scanner was used for a diagnostic.</param>    
    public static void DiagnosticUsage(double seconds)
    {
        totalScannerUsageTime += seconds;
    }


    /// <summary>
    /// Continuously processes patients in the statistics queue, updating the total waiting times for each priority group.
    /// This method locks the statistics section to ensure thread safety while updating the accumulated data for emergency, urgency,
    /// and general consultation patients. It terminates when all patients have been processed.
    /// </summary>
    public static void StatsProcess()
    {
        while (true)
        {
            // Lock the statistics section to ensure thread safety when accessing shared resources
            lock (statslock)
            {
                while (statsPatientList.Count == 0)
                {
                    // If all patients have been processed, exit the loop and stop processing
                    if (PriorityProgram.TotalPatients == PriorityProgram.processedPatients) return;

                    Monitor.Wait(statslock); // Thread waiting spot
                }

                Patient patient = statsPatientList.Dequeue();

                // Update the total waiting times based on the patient's priority
                switch (patient.Priority)
                {
                    case PatientPriority.Emergency:
                        totalEmergencyPatients++;
                        totalEmergencyWaitingTime += patient.WaitingTime;
                        break;
                    case PatientPriority.Urgency:
                        totalUrgencyPatients++;
                        totalUrgencyWaitingTime += patient.WaitingTime;
                        break;
                    case PatientPriority.GeneralConsultation:
                        totalGeneralPatients++;
                        totalGeneralWaitingTime += patient.WaitingTime;
                        break;
                }

                // Notify all other threads waiting on the stats lock
                Monitor.PulseAll(statslock);
            }
        }
    }
}