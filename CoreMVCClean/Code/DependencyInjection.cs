using Core.Library.Clean.AdditionalService;

namespace Core.MVC.Clean
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddHttpContextAccessor();

            // --------------------------------------------------
            // The URL is configured in appsettings.json
            // --------------------------------------------------

            var bookAPIUrl = configuration["bookAPIUrl"];
            var personAPIUrl = configuration["personAPIUrl"];

            // --------------------------------------------------
            // Register the API services used as dependencies by the MCP tools.
            // --------------------------------------------------

            services.AddHttpClient<BookDirector>(client =>
            {
                client.BaseAddress = new Uri(bookAPIUrl);
            });

            services.AddHttpClient<PersonDirector>(client =>
            {
                client.BaseAddress = new Uri(personAPIUrl);
            });

            return services;
        }
    }
}
