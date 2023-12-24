namespace Demo.DependencyInjections
{
    public class ServiceA : IServiceA
    {
        private Guid Id;
        public ServiceA()
        {
            Id = Guid.NewGuid();
        }
        public string GetId()
        {
            return Id.ToString();
        }
    }
}
