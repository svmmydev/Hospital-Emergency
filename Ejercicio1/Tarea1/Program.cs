class Doctor(int id)
{
    public int Id { get; set; } = id;
    public bool IsAvailable {get; set;} = true;
}

internal class Program
{
    static int totalPatients = 4;
    static List<Doctor> availableDoctors =
    [
        new Doctor(1),
        new Doctor(2),
        new Doctor(3),
        new Doctor(4)
    ];

    static SemaphoreSlim consultSem = new SemaphoreSlim(4);
    static readonly object locker = new object();
    static Random rnd = new Random();

    private static void Main(string[] args)
    {
        //Code
    }
}