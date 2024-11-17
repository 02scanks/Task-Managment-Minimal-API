namespace MinimalTaskManagingAPI.Models.DTO
{
    public class CreateTaskDTO
    {
        public string TaskName { get; set; }
        public string Notes { get; set; }
        public DateTime CompleteDate { get; set; }
    }
}
