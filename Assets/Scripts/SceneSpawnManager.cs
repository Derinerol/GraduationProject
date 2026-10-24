
public static class SceneSpawnManager
{
    public static string PendingSpawnID { get; private set; }

    public static void SetNextSpawn(string spawnID)
    {
        PendingSpawnID = spawnID;
    }

    public static void ClearPendingSpawn()
    {
        PendingSpawnID = null;
    }
}
