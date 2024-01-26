namespace Application
{
	public class GenericData <TViewModel> where TViewModel : class
	{
        public int Count { get; set; }
        public List<TViewModel> Data { get; set; } = new();
    }
}