
namespace HospitalUrgencias.Hospital.Helpers;
using HospitalUrgencias.Hospital.Models;
using HospitalUrgencias.Hospital.Services;


/// <summary>
/// Represents the UI of the simultaion through the console.
/// </summary>
public static class ConsoleView
{
    /// <summary>
    /// Shows a welcome message.
    /// </summary>
    public static void ShowWelcomeMessage()
    {
        Console.WriteLine("\n# PATIENTS ARE ENTERING THE HOSPITAL #");
        Console.WriteLine("-----------------------------------------------------------------");

    }


    /// <summary>
    /// Shows an ending program message.
    /// </summary>
    public static void ShowExitMessage()
    {
        Console.WriteLine("\n-----------------------------------------------------------------");
        Console.WriteLine("# ALL PATIENTS HAVE BEEN TREATED #\n");
    }

    
    /// <summary>
    /// Shows a block with all required statistics.
    /// </summary>
    public static void ShowStatMessage()
    {
        Console.WriteLine(
            "\n# STATISTICS #" +
            "\n-----------------------------------------------------------------\n" +
            "\nPatients treated:" +
            $"\n -Emergency: {Statistics.totalEmergencyPatients}" +
            $"\n -Urgency: {Statistics.totalUrgencyPatients}" +
            $"\n -General: {Statistics.totalGeneralPatients}" +
            "\n\nAverage waiting time:" +
            $"\n -Emergency: {Statistics.averageEmergencyWaitingTime}" +
            $"\n -Urgency: {Statistics.averageUrgencyWaitingTime}" +
            $"\n -General: {Statistics.averageGeneralWaitingTime}" +
            $"\n\nAverage diagnostic scanner usage: {Statistics.averageScannerUsedTime}%" +
            "\n\n-----------------------------------------------------------------\n"
        );
    }


    /// <summary>
    /// Displays the current status of a patient, including their consultation status, assigned doctor, 
    /// diagnostic status, and priority information (if requested).
    /// </summary>
    /// <param name="patient"></param>
    /// <param name="Doctor"></param>
    /// <param name="CTScanner"></param>
    /// <param name="showPriorityMessage"></param>
    public static void ShowHospitalStatusMessage(Patient patient, Doctor? Doctor = null, CTScanner? CTScanner = null, bool showPriorityMessage = false)
    {
        string statusMsg;
        string priorityMsg = "";

        switch (patient.Status)
        {
            case PatientStatus.WaitingConsultation:
                statusMsg = $"| Waiting for a consultation call";
                break;
            case PatientStatus.InConsultation:
                statusMsg = $"| Assigned: {Doctor?.ReferenceName} " +
                            $"| Waiting duration: {patient.WaitingTime}s";
                break;
            case PatientStatus.Finished:
                if (patient.DiagnosticCompleted)
                {
                    statusMsg = $"| Diagnostic completed ({CTScanner?.ReferenceName} is now free)";
                }
                else
                {
                    statusMsg = $"| Consultation duration: {patient.ConsultationTime}s " +
                                $"({Doctor?.ReferenceName} is now free)";
                }

                break;
            case PatientStatus.WaitingDiagnostic:
                if (patient.RequiresDiagnostic && !patient.DiagnosticCompleted)
                {
                    statusMsg = $"| Consultation duration: {patient.ConsultationTime}s " +
                                $"({Doctor?.ReferenceName} is now free)";

                    statusMsg += $" | Waiting for a diagnostic CT Scanner test";
                }
                else
                {
                    statusMsg = $"| Entering {CTScanner?.ReferenceName}";
                }
                
                break;
            default:
                statusMsg = "";
                break;
        }

        if (showPriorityMessage) priorityMsg = $"| Priority: {patient.Priority} ";

        Console.WriteLine(
            $"\n| Patient {patient.Id} " +
            $"| Arrived as {patient.HospitalArrival} " +
            $"{priorityMsg}" +
            $"| Status: {patient.Status} " +
            $"{statusMsg} |"
        );
    }
}