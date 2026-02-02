namespace cafeteriaKFE.Core.Users.Request
{
    public class GetUsersRequest
    {
        public long userId { get; set; }
        public string name { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public DateTime createdAt { get; set; }
    }
}
