namespace MinimalTaskManagingAPI.Models
{
    public class Task
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string TaskName { get; set; }
        public string Notes { get; set; }
        public bool isComplete { get; set; }
        public DateTime CompleteDate { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ModificationDate { get; set; }


    }
}
