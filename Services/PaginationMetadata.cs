namespace NoteManagementAPI.Services
{
    public class PaginationMetadata
    {
        public int TotalItemCount { get; set; }
        public int TotalPageCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }

        public PaginationMetadata(int totalItemCount, int pageSize, int currentPage)
        {
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");
            }

            if (currentPage < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(currentPage), "Current page must be greater than 0.");
            }

            TotalItemCount = totalItemCount;            
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPageCount = (int)Math.Ceiling((double)totalItemCount / pageSize);
        }
    }
}
