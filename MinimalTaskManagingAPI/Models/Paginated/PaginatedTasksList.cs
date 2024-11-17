namespace MinimalTaskManagingAPI.Models.Paginated
{
    public class PaginatedTasksList
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public int TaskCount { get; set; }
        public List<Task> Tasks { get; set; }
    }
}
