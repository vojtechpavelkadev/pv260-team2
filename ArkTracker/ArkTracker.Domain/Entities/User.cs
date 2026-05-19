namespace ArkTracker.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }

        private User() { Username = string.Empty; PasswordHash = string.Empty; }

        public User(string username, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty.");
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be empty.");

            Id = Guid.NewGuid();
            Username = username;
            PasswordHash = passwordHash;
        }
    }
}
