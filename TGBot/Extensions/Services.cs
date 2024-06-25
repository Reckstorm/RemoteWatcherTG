using Application.RProcesses;
using Microsoft.Extensions.DependencyInjection;

namespace TGBot.Extensions
{
    public static class Services
    {
        public static IServiceProvider CreateProvider()
        {
            var collection = new ServiceCollection();

            collection.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(List.Handler).Assembly));

            return collection.BuildServiceProvider();
        }
    }
}