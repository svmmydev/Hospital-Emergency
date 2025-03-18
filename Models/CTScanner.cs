
namespace HospitalUrgencias.Models;

/// <summary>
/// Represents a CT Scanner in the hospital.
/// </summary>
public class CTScanner
{
    // Properties
    public int Id {get; private set;}
    public string ReferenceName {get; private set;}
    public bool IsAvailable {get; set;} = true;


    // Common variables
    static readonly object locker = new object();


    /// <summary>
    /// Initializes a new instance of the CT Scanner class.
    /// </summary>
    /// <param name="Id">The unique identification number of the CT Scanner.</param>
    public CTScanner(int Id)
    {
        this.Id = Id;
        ReferenceName = $"CT Scanner {Id}";
    }


    /// <summary>
    /// Selects a random available CT Scanner.
    /// This method blocks execution until a CT Scanner becomes available.
    /// Access to the CT Scanner list is synchronized to ensure data consistency and avoid race conditions.
    /// </summary>
    /// <returns>A CT Scanner object representing the assigned CT Scanner.</returns>
    public static CTScanner AssignCTScanner()
    {
        CTScanner selectedCTScanner;

        while (true)
        {
            lock (locker)
            {
                var availableCTScanner = Hospital.CTScannerList.Where(d => d.IsAvailable).ToList(); 

                if (availableCTScanner.Count > 0)
                {
                    selectedCTScanner = availableCTScanner[Hospital.rnd.Next(availableCTScanner.Count)];
                    selectedCTScanner.IsAvailable = false;
                    return selectedCTScanner;
                }
            }
        }
    }


    /// <summary>
    /// Marks the CT Scanner as available after a patient finishes the medical test.
    /// </summary>
    public void ReleaseCTScanner()
    {
        lock (locker)
        {
            IsAvailable = true;
        }
    }
}