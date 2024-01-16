using Application.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static void AddService(this IServiceCollection services)

        {
            services.AddScoped<IProductService, ProductService>();
        }
    }
}
