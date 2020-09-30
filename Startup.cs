using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ServerApp.Models;
using ServerApp.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ServerApp
{
    public class Startup
    {
        private const string AllowAllCors = "AllowAll";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString =
                Configuration["ConnectionStrings:DefaultConnection"];

            #region App Services

            services.AddDbContext<BenefitsDataContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IBenefitsService, BenefitsService>();
            services.AddScoped<IPaycheckService, PaycheckService>();

            #endregion

            services.AddControllersWithViews()
                .AddJsonOptions(opts => { opts.JsonSerializerOptions.IgnoreNullValues = true; })
                .AddNewtonsoftJson();
            services.AddSwaggerGen(options =>
            {
                options.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["action"]}");
                options.SwaggerDoc("v1",
                    new OpenApiInfo {Title = "PCTYBenefits API", Version = "v1"});
            });

            services.AddCors(options =>
            {
                options.AddPolicy(AllowAllCors,
                    builder =>
                    {
                        builder.AllowAnyHeader();
                        builder.AllowAnyMethod();
                        builder.AllowAnyOrigin();
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseCors(AllowAllCors);
            

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json",
                    "PCTY Benefits API");
            });

            app.UseSpa(spa =>
            {
                var strategy = Configuration
                    .GetValue<string>("DevTools:SpaConnectionStrategy");
                Console.WriteLine("Starting SPA strategy: " + strategy);
                if (strategy == "proxy")
                {
                    spa.UseProxyToSpaDevelopmentServer("http://127.0.0.1:4200");
                }
                else if (strategy == "managed")
                {
                    spa.Options.SourcePath = "./../ClientApp";
                    spa.UseAngularCliServer("start");
                }
            });
            SeedData.Initialize(services);
        }
    }
}