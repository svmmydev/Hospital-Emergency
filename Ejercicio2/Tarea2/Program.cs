
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
        Hospital.HospitalProgram(PatientProcess);
    }


    private static void PatientProcess(Patient patient)
    {
        Hospital.consultSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        patient.Status = PatientStatus.InConsultation;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);
        if (patient.RequiresDiagnostic) Hospital.DiagnosticQueue.Add(patient);
        Thread.Sleep(patient.ConsultationTime * 1000);

        assignedDoctor.ReleaseDoctor();
        Hospital.consultSem.Release();

        patient.Status = PatientStatus.Finished;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);

        if (patient.RequiresDiagnostic)
        {
            CTScanner assignedCTScanner;

            lock (Hospital.queueLock)
            {
                while (!(Hospital.PatientQueue.TryPeek(out Patient? firstPatient) && firstPatient == patient))
                {
                    Monitor.Wait(Hospital.queueLock);
                }
                
                Hospital.DiagnosticQueue.Take();
                
                Hospital.scannerSem.Wait();
                assignedCTScanner = CTScanner.AssignCTScanner();

                Monitor.PulseAll(Hospital.queueLock);
            }
            
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