
namespace HospitalUrgencias.Models;

public static class ConsoleView
{
    public static void ShowWelcomeMessage()
    {
        Console.WriteLine("\nPatients are entering the hospital..\n");
    }


    public static void ShowHospitalStatusMessage(Patient patient, Doctor assignedDoctor, CTScanner? cTScanner = null, string extraData = "")
    {
        string statusMsg;

        switch (patient.Status)
        {
            case PatientStatus.InConsultation:
                statusMsg = $"| Assigned Doctor: {assignedDoctor.ReferenceName} " +
                            $"| Waiting duration: {patient.WaitingTime}";
                break;
            case PatientStatus.WaitingDiagnostic:
                statusMsg = $"| Waiting diagnostic results ({cTScanner?.ReferenceName})";
                break;
            case PatientStatus.Finished:
                statusMsg = $"({assignedDoctor.ReferenceName} is now free) " +
                            $"| Consultation duration: {patient.ConsultationTime}" ;
                break;
            default:
                statusMsg = "";
                break;
        }

        if (!string.IsNullOrEmpty(extraData))
        {
            statusMsg += " | " + extraData;
        }

        Console.WriteLine(
            $"| Patient {patient.Id} " +
            $"| Arrived as {patient.HospitalArrival} " +
            $"| Status: {patient.Status} " +
            $"{statusMsg} |"
        );
    }
}