
namespace HospitalUrgencias.Hospital.Helpers;

public static class RandomIdGenerator
{
    private static readonly HashSet<int> usedIds = new HashSet<int>();
    private static readonly Random rnd = new Random();
    private static readonly object lockObj = new object();


    public static int GetUniqueId(int maxId)
    {
        lock (lockObj)
        {
            int id;
            do
            {
                id = rnd.Next(1, maxId+1);
            }
            while (!usedIds.Add(id));

            return id;
        }
    }
}