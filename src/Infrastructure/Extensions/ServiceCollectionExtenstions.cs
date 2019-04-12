namespace BasicBot.Infrastructure.Extensions
{
    using System.Linq;
    using System.Reflection;

    using BasicBot.Services;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtenstions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            Assembly
                .GetAssembly(typeof(IService))
                .GetTypes()
                .Where(t => t.IsClass && t.GetInterfaces().Any(i => i.Name == $"I{t.Name}") && !t.IsAbstract && !t.IsGenericType)
                .Select(t => new
                {
                    Interface = t.GetInterface($"I{t.Name}"),
                    Implementation = t,
                })
                .ToList()
                .ForEach(s => services.AddScoped(s.Interface, s.Implementation));

            return services;
        }
    }
}
