
namespace HospitalUrgencias.Hospital.Helpers;


/// <summary>
/// Represents a custom ID Randomizer.
/// </summary>
public static class RandomIdGenerator
{
    // Common variables.
    private static readonly HashSet<int> usedIds = new HashSet<int>();
    private static readonly Random rnd = new Random();
    private static readonly object lockObj = new object();


    /// <summary>
    /// Generates a unique identifier that hasn't been used before, within the range from 1 to maxId.
    /// This method ensures thread-safety while generating the ID by locking the operation.
    /// </summary>
    /// <param name="maxId">The maximum possible ID value that can be generated.</param>
    /// <returns>A unique ID that is not already used.</returns>
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