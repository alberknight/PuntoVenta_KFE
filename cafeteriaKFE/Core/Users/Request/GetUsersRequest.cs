namespace cafeteriaKFE.Core.Users.Request
{
    public class GetUsersRequest
    {
        public string name { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public DateTime createdAt { get; set; }
    }
}
