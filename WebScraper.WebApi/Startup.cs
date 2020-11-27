using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ML;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using System;
using NLog.Extensions.Logging;
using WebScraper.Data;
using WebScraper.Core.Helpers;
using WebScraper.Core.Factories;
using WebScraper.Core.Loaders;
using WebScraper.Core;
using WebScraper.Core.ML;
using WebScraper.Core.Parsers;

namespace WebScraper.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IWebHostEnvironment environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"parserSettings.json", optional: false, reloadOnChange: true)
                .AddConfiguration(configuration);

            Configuration = builder.Build();

            NLog.LogManager.Configuration = new NLogLoggingConfiguration(Configuration.GetSection("NLog"));

            environment = env;
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

            services.AddSingleton<PriceParser>();
            services.AddSingleton<MLPriceParser>();
            services.AddTransient<PriceParserFactory>();

            services.AddPredictionEnginePool<PriceData, PricePrediction>()
                .FromFile(modelName: "PriceDetectionModel", filePath: Path.Combine(environment.ContentRootPath, "ML/MLModel.zip"), watchForChanges: true);

            services.AddTransient<HttpLoader>();
            services.AddSingleton<SelenuimLoader>();
            services.AddSingleton<PuppeteerLoader>();
            services.AddSingleton<HeadlessPuppeteerLoader>();
            services.AddTransient<HtmlLoaderFactory>();

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
