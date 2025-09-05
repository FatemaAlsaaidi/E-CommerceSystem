namespace E_CommerceSystem.Services.Email
{
    public sealed class EmailSettings
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FromName { get; set; } = default!;
        public string FromEmail { get; set; } = default!;
    }
}