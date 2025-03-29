
namespace HospitalUrgencias.Hospital.Services;

using HospitalUrgencias.Hospital.Models;

public class Statistics
{
    public static int totalEmergencyPatients, totalUrgencyPatients, totalGeneralPatients;
    public static int totalEmergencyWaitingTime, totalUrgencyWaitingTime, totalGeneralWaitingTime;
    public static double averageEmergencyWaitingTime, averageUrgencyWaitingTime, averageGeneralWaitingTime;

    public static double totalScannerUsageTime, averageScannerUsedTime;

    public static Queue<Patient> statsPatientList = new Queue<Patient>();
    public static object statslock = new object();
    public static TimeSpan totalSessionTime;


    public static void CalculateStats()
    {
        averageEmergencyWaitingTime = totalEmergencyPatients > 0 ? totalEmergencyWaitingTime / totalEmergencyPatients : 0;
        averageUrgencyWaitingTime = totalUrgencyPatients > 0 ? totalUrgencyWaitingTime / totalUrgencyPatients : 0;
        averageGeneralWaitingTime = totalGeneralPatients > 0 ? totalGeneralWaitingTime / totalGeneralPatients : 0;

        averageScannerUsedTime = totalSessionTime.TotalSeconds > 0
        ? (int)(totalScannerUsageTime / (Hospital.CTScannerList.Count * totalSessionTime.TotalSeconds) * 100)
        : 0;
    }


    public static void DiagnosticUsage(double seconds)
    {
        totalScannerUsageTime += seconds;
    }


    public static void StatsProcess()
    {
        while (true)
        {
            lock (statslock)
            {
                while (statsPatientList.Count == 0)
                {
                    if (PriorityProgram.TotalPatients == PriorityProgram.processedPatients) return;

                    Monitor.Wait(statslock);
                }

                Patient patient = statsPatientList.Dequeue();

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

                Monitor.PulseAll(statslock);
            }
        }
    }
}