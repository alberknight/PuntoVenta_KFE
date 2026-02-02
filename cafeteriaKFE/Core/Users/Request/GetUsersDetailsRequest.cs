namespace cafeteriaKFE.Core.Users.Request
{
    public class GetUsersDetailsRequest
    {
        public long userId { get; set; }
        public string name { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public DateTime createdAt { get; set; }
        public string passhwordHash { get; set; }
        public string phoneNumber { get; set; }
    }
}
