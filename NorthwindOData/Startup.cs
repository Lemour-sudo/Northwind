using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.EntityFrameworkCore;
using Packt.Shared;

namespace NorthwindOData
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            string databasePath = Path.Combine("../database", "Northwind.db");
            services.AddDbContext<Northwind>(
                options => options
                    .UseSqlite($"Data Source={databasePath}")
                    .UseLoggerFactory(new ConsoleLoggerFactory())
            );

            services.AddControllers()
                .AddOData(options => options
                    // register OData models including multiple verions
                    .AddRouteComponents(routePrefix: "catalog", model: GetEdmModelForCatalog())
                    .AddRouteComponents(routePrefix: "ordersystem", model: GetEdmModelForOrderSystem())
                    .AddRouteComponents(routePrefix: "v{version}", model: GetEdmModelForCatalog())

                    // enable query options
                    .Select() // enable $select for projection
                    .Expand() // enable $expand to navigate to related entities
                    .Filter() // enable $filter
                    .OrderBy() // enable $orderby
                    .SetMaxTop(100) // enable $top
                    .Count() // enable $count
                );
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NorthwindOData", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NorthwindOData v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        IEdmModel GetEdmModelForCatalog()
        {
            ODataConventionModelBuilder builder = new();
            builder.EntitySet<Category>("Categories");
            builder.EntitySet<Product>("Products");
            builder.EntitySet<Supplier>("Suppliers");
            return builder.GetEdmModel();
        }

        IEdmModel GetEdmModelForOrderSystem()
        {
            ODataConventionModelBuilder builder = new();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            builder.EntitySet<Employee>("Employees");
            builder.EntitySet<Product>("Products");
            builder.EntitySet<Shipper>("Shippers");
            return builder.GetEdmModel();
        }
    }
}
