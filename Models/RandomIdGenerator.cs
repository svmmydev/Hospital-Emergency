
namespace HospitalUrgencias.Models;

public static class RandomIdGenerator
{
    private static readonly HashSet<int> usedIds = new();
    private static readonly Random rnd = new();
    private static readonly object lockObj = new();


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