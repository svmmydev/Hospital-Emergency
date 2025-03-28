
namespace HospitalUrgencias.Hospital.Helpers;
using HospitalUrgencias.Hospital.Models;

public static class ConsoleView
{
    public static void ShowWelcomeMessage()
    {
        Console.WriteLine("\n# PATIENTS ARE ENTERING THE HOSPITAL #");
        Console.WriteLine("-----------------------------------------------------------------");

    }


    public static void ShowExitMessage()
    {
        Console.WriteLine("\n-----------------------------------------------------------------");
        Console.WriteLine("# ALL PATIENTS HAVE BEEN TREATED #\n");
        Console.WriteLine("\nClosing program..\n");
    }


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
                statusMsg = $"| Consultation duration: {patient.ConsultationTime}s " +
                            $"({Doctor?.ReferenceName} is now free)";

                if (patient.RequiresDiagnostic) statusMsg += $" | Waiting for a diagnostic CT Scanner test";
                
                break;
            case PatientStatus.WaitingDiagnostic:
                if (patient.RequiresDiagnostic) statusMsg = $"| Entering {CTScanner?.ReferenceName}";
                else statusMsg = $"| Diagnostic completed ({CTScanner?.ReferenceName} is now free)";

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