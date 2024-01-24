namespace Application
{
    public class Page
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public int SkipNumber => (PageIndex - 1) * PageSize;
    }
}
