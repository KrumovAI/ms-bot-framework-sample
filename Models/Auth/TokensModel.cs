namespace BasicBot.Models
{
    using System;

    public class TokensModel
    {
        public TokensModel()
        {
            this.IsAuthenticated = false;
        }

        public string AccessToken { get; set; }

        public string IdToken { get; set; }

        public string RefreshToken { get; set; }

        public int ExpiresIn { get; set; }

        public DateTime ExpiresAt { get; set; }

        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public Guid SchoolId { get; set; }

        public bool IsAuthenticated { get; set; }
    }
}
