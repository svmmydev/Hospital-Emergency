﻿
using System.Collections.Concurrent;

namespace HospitalUrgencias.Models;

//TODO: Ordenar todo el código según semántica
public static class Hospital
{
    public static readonly SemaphoreSlim consultationSem = new SemaphoreSlim(4);
    public static TurnTicket diagnosticTicketTurn = new TurnTicket();
    public static readonly SemaphoreSlim scannerSem = new SemaphoreSlim(2);
    public static TurnTicket consultationTicketTurn = new TurnTicket();
    public static readonly Random rnd = new Random();
    public static readonly object queueLock = new object();


    public const int patientArrivalInterval = 2000;
    public const int medicalTestTime = 15000;


    public static ConcurrentQueue<Patient> PatientQueue = new ConcurrentQueue<Patient>();
    public static BlockingCollection<Patient> DiagnosticQueue = new BlockingCollection<Patient>(PatientQueue);
    private static List<Patient> PatientList = new List<Patient>();
    private static List<Thread> PatientThreads = new List<Thread>();
    private static List<Thread> DiagnosticThreads = new List<Thread>();

    
    // Doctor list
    public static readonly List<Doctor> DoctorList = new List<Doctor>
    {
        new Doctor(1),
        new Doctor(2),
        new Doctor(3),
        new Doctor(4)
    };


    // CTScanner list
    public static readonly List<CTScanner> CTScannerList = new List<CTScanner>
    {
        new CTScanner(1),
        new CTScanner(2)
    };


    public static void HospitalProgram(Action<Patient> action, int totalPatients)
    {
        ConsoleView.ShowWelcomeMessage();

        for (int i = 0; i < CTScannerList.Count; i++)
        {
            Thread diagnosticThread = new Thread(DiagnosticProcess);
            DiagnosticThreads.Add(diagnosticThread);
            diagnosticThread.Start();
        }

        // Simulates the arrival of patients at intervals.
        for (int i = 1; i <= totalPatients; i++)
        {
            int arrivalOrderNum = i;
            int Id = RandomIdGenerator.GetUniqueId(1000);

            int consultationTicket = consultationTicketTurn.GetTicket();
            int consultationTime = rnd.Next(5,16);

            Patient patient = new Patient(Id, arrivalOrderNum, consultationTicket, consultationTime);
            PatientList.Add(patient);

            Thread patientThread = new Thread(() => action(patient));
            PatientThreads.Add(patientThread);
            patientThread.Start();

            Thread.Sleep(patientArrivalInterval);
        }

        foreach(Thread thread in PatientThreads)
        {
            thread.Join();
        }

        DiagnosticQueue.CompleteAdding();

        foreach(Thread thread in DiagnosticThreads)
        {
            thread.Join();
        }

        ConsoleView.ShowExitMessage();
    }


    public static void PatientProcess(Patient patient)
    {
        ConsoleView.ShowHospitalStatusMessage(patient);

        consultationTicketTurn.WaitTurn(patient.HospitalArrival);

        consultationSem.Wait();
        Doctor assignedDoctor = Doctor.AssignDoctor();

        patient.Status = PatientStatus.InConsultation;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);

        consultationTicketTurn.Next();

        if (patient.RequiresDiagnostic)
        {
            patient.DiagnosticTicket = diagnosticTicketTurn.GetTicket();
            DiagnosticQueue.Add(patient);
        }

        Thread.Sleep(patient.ConsultationTime * 1000);
        
        patient.Status = PatientStatus.Finished;
        ConsoleView.ShowHospitalStatusMessage(patient, Doctor: assignedDoctor);

        assignedDoctor.ReleaseDoctor();
        consultationSem.Release();

        if (patient.RequiresDiagnostic)
        {
            lock (queueLock)
            {
                Monitor.PulseAll(queueLock);
            }
        }
    }


    private static void DiagnosticProcess()
    {
        foreach (var patient in DiagnosticQueue.GetConsumingEnumerable())
        {
            diagnosticTicketTurn.WaitTurn(patient.DiagnosticTicket);

            lock (queueLock)
            {
                while (patient.Status != PatientStatus.Finished)
                {
                    Monitor.Wait(queueLock);
                }
            }

            CTScanner assignedCTScanner = CTScanner.AssignCTScanner();
            scannerSem.Wait();

            patient.Status = PatientStatus.WaitingDiagnostic;
            ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);

            diagnosticTicketTurn.Next();

            Thread.Sleep(medicalTestTime);

            patient.RequiresDiagnostic = false;
            ConsoleView.ShowHospitalStatusMessage(patient, CTScanner: assignedCTScanner);

            assignedCTScanner.ReleaseCTScanner();
            scannerSem.Release();

            lock (queueLock)
            {
                Monitor.PulseAll(queueLock);
            }
        }
    }


    private static bool CheckingExistentId(int Id)
    {
        foreach(Patient patient in PatientList)
        {
            if(patient.Id == Id) return true;
        }

        return false;
    }
}
