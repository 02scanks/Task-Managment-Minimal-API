namespace MinimalTaskManagingAPI.Models.Paginated
{
    public class PaginatedUserList
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public int UserCount { get; set; }
        public List<User> Users { get; set; }
    }
}
