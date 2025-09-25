using System;
using System.Linq;
using System.Net.Http;
using DatabaseAdapters;
using DatabaseAdapters.Repositories;
using Domain;
using Domain.Repositories;
using Domain.Services.Util;
using ExternalServiceAdapters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TemperatureSystem;
using TemperatureSystem.HostedServices;

namespace IntegrationTests
{
    // NOTE: These tests are currently failing due to a known issue in .NET 9.
    // The PipeWriter 'ResponseBodyPipeWriter' does not implement PipeWriter.UnflushedBytes.
    // See: https://github.com/dotnet/runtime/issues/108075
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
                    services.RemoveAll(typeof(IHostedService));
                    services.AddDbContext<IDatabaseContext, DatabaseContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });

                    services.AddDomain();
                    services.AddExternalServices();
                    services.AddScoped<IUnitOfWork, UnitOfWork>();
                    services.AddScoped<IMeasurementRepository, MeasurementRepository>();
                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();
                    services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
                    services.AddScoped<ISensorRepository, SensorRepository>();
                    services.AddLogging();
                    services.AddMvc();
                    services.AddControllers();

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<IDatabaseContext>() as DbContext;
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