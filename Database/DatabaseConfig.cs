namespace GymWorkoutLogApi.Database
{
    public class DatabaseConfig
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string GetConnectionString()
            => $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};";
    }
}
