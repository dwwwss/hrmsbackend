public class BlacklistService
{
    private readonly HashSet<string> _tokenBlacklist = new HashSet<string>();
    private readonly object _lockObject = new object();

    public bool IsTokenBlacklisted(string token)
    {
        lock (_lockObject)
        {
            return _tokenBlacklist.Contains(token);
        }
    }

    public void AddToBlacklist(string token)
    {
        lock (_lockObject)
        {
            _tokenBlacklist.Add(token);
        }
    }
}


