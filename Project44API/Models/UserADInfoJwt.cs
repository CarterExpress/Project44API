namespace Project44API.Models
{
    public class UserADInfoJwt
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}