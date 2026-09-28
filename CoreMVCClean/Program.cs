using Core.MVC.Clean;
using Serilog;

namespace Core.MVC.Clean
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --------------------------------------------------
            // Configure Serilog
            // --------------------------------------------------
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
            );

            // --------------------------------------------------
            // Add services to the container.
            // --------------------------------------------------
            builder.Services.AddControllersWithViews();

            // --------------------------------------------------
            // Dependency Injection - Application Services
            // --------------------------------------------------
            builder.Services.AddApplicationServices(builder.Configuration);

            var app = builder.Build();

            // --------------------------------------------------
            // CRITICAL: Place this at the very top of the pipeline 
            // so it can catch exceptions thrown by any middleware or controllers below it.
            // --------------------------------------------------

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<GlobalExceptionHandler>();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                // --------------------------------------------------
                //Stands for HTTP Strict Transport Security.
                //It tells web browsers that your site should only be accessed via HTTPS, forcing secure connections
                // --------------------------------------------------
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
