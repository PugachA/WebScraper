using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebScraper.WebApi.Models;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using System;
using NLog.Extensions.Logging;
using WebScraper.WebApi.Helpers;
using WebScraper.WebApi.Models.Factories;

namespace WebScraper.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddConfiguration(configuration);

            Configuration = builder.Build();

            NLog.LogManager.Configuration = new NLogLoggingConfiguration(Configuration.GetSection("NLog"));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<ProductWatcherContext>(
                    options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")),
                    ServiceLifetime.Transient,
                    ServiceLifetime.Transient);

            services.AddTransient<IConfiguration>(provider => Configuration);
            services.AddTransient<HangfireSchedulerClient>();
            services.AddTransient<PriceParserFactory>();
            services.AddTransient<ProductWatcherManager>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Web Scraper", Version = "v1" });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"./v1/swagger.json", "Web Scraper API");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
