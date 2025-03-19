
namespace HospitalUrgencias.Models;

public static class ConsoleView
{
    public static void ShowWelcomeMessage()
    {
        Console.WriteLine("\nPatients are entering the hospital..\n");
    }


    public static void ShowHospitalStatusMessage(Patient patient, Doctor? Doctor = null, CTScanner? CTScanner = null, string? extraMsg = null)
    {
        string statusMsg;

        switch (patient.Status)
        {
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

        Console.WriteLine(
            $"| Patient {patient.Id} " +
            $"| Arrived as {patient.HospitalArrival} " +
            $"| Status: {patient.Status} " +
            $"{statusMsg} |"
        );
    }
}