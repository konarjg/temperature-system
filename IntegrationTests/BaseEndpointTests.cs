using System;
using System.Linq;
using System.Net.Http;
using DatabaseAdapters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TemperatureSystem;

namespace IntegrationTests
{
    public class BaseEndpointTests : IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        protected readonly HttpClient Client;

        protected BaseEndpointTests()
        {
            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(DbContextOptions<DatabaseContext>));
                    services.AddDbContext<DatabaseContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<DatabaseContext>();
                    db.Database.EnsureCreated();
                });
            });
            Client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _factory.Dispose();
        }
    }
}