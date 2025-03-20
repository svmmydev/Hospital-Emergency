
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
        Hospital.HospitalProgram(PatientArrival);
    }


    /// <summary>
    /// Simulates the arrival of a patient and assigns them to a doctor for a consultation.
    /// </summary>
    /// <param name="Patient">The patient with all of his properties.</param>
    private static void PatientArrival(Patient patient)
    {
        Hospital.consultSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        patient.Status = PatientStatus.InConsultation;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);
        Thread.Sleep(patient.ConsultationTime * 1000);

        assignedDoctor.ReleaseDoctor();
        Hospital.consultSem.Release();

        patient.Status = PatientStatus.Finished;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);

        if (patient.RequiresDiagnostic)
        {
            while (true)
            {
                if (Hospital.PatientQueue.TryPeek(out Patient? firstPatient) && firstPatient == patient)
                {
                    Hospital.DiagnosticQueue.Take();
                    break;
                }
                Thread.Sleep(100);
            }

            Hospital.scannerSem.Wait();
            CTScanner assignedCTScanner = CTScanner.AssignCTScanner();

            patient.Status = PatientStatus.WaitingDiagnostic;
            ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);
            Thread.Sleep(Hospital.medicalTestTime);

            patient.RequiresDiagnostic = false;
            ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);
            
            assignedCTScanner.ReleaseCTScanner();
            Hospital.scannerSem.Release();
        }
    }
}