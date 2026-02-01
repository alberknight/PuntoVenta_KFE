namespace cafeteriaKFE.Core.Users.Response
{
    public class UpdateUserResponse
    {
        public int userId { get; set; }
        public string name { get; set; } = null!;
        public string lastname { get; set; } = null!;
        public string? phoneNumber { get; set; }
        public string email { get; set; } = null!;
    }
}
