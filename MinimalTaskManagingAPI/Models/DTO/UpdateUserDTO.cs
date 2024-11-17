namespace MinimalTaskManagingAPI.Models.DTO
{
    public class UpdateUserDTO
    {
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
