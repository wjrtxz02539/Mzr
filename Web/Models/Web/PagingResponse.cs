namespace Mzr.Web.Models.Web
{
    public class PagingResponse<T> where T : class
    {
        public List<T> Items { get; set; }
        public PagingMetaData MetaData { get; set; }

        public PagingResponse()
        {
            MetaData = new PagingMetaData() { CurrentPage = 1, PageSize = 0, TotalCount = 0 };
            Items = new();
        }
        public PagingResponse(List<T> items, int totalCount, int pageSize, int currentPage)
        {
            MetaData = new()
            {
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = currentPage
            };
            Items = items;
        }
    }
}
