public class AuthService
{
    private static readonly List<User> _users = new();
    private static readonly object _lock = new();

    public bool Register(User user)
    {
        lock (_lock)
        {
            if (_users.Any(u => u.Username == user.Username || u.Email == user.Email))
            {
                return false; // User already exists
            }
            _users.Add(user);
            return true;
        }
    }

    public bool Login(string email, string password)
    {
        lock (_lock)
        {
            return _users.Any(u => u.Email == email && u.Password == password);
        }
    }
}