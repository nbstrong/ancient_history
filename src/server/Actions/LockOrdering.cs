namespace WurmStyleGame.Server.Actions;

public enum LockResourceClass
{
    Chunk = 1,
    Entity = 2,
    Container = 3
}

public static class LockOrdering
{
    public static int Compare((LockResourceClass Kind, string Key) a, (LockResourceClass Kind, string Key) b)
    {
        int kind = a.Kind.CompareTo(b.Kind);
        if (kind != 0)
        {
            return kind;
        }

        return string.CompareOrdinal(a.Key, b.Key);
    }
}
