using System;
using DatabaseAdapters;
using DatabaseAdapters.Repositories;
using DatabaseAdapters.Repositories.Test;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.External;
using Domain.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IntegrationTests;

public abstract class BaseServiceTests : IDisposable {
    private readonly IServiceScope _scope;
    protected readonly TestDatabaseContext DbContext;
    protected readonly IServiceProvider ServiceProvider;

    protected BaseServiceTests() {
        var services = new ServiceCollection();

        // Database Setup
        services.AddDbContext<TestDatabaseContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // CRITICAL FIX: Register TestDatabaseContext as the provider for both interfaces.
        // This ensures Repositories (which need IDatabaseContext) and UnitOfWork (which needs DbContext)
        // receive the SAME database instance.
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<TestDatabaseContext>());
        services.AddScoped<IDatabaseContext>(provider => provider.GetRequiredService<TestDatabaseContext>());
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IMeasurementRepository, MeasurementRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Services
        services.AddScoped<IMeasurementService, MeasurementService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        // Mocks for external dependencies that should not be part of integration tests
        services.AddSingleton(new Mock<IPasswordSecurity>().Object);
        services.AddSingleton(new Mock<ITokenGenerator>().Object);
        services.AddSingleton(new Mock<IEmailService>().Object);
        services.AddSingleton(new Mock<IEmailSettingsProvider>().Object);

        // Create a scope for the test
        var serviceProvider = services.BuildServiceProvider();
        _scope = serviceProvider.CreateScope();
        ServiceProvider = _scope.ServiceProvider;
        DbContext = ServiceProvider.GetRequiredService<TestDatabaseContext>();
    }

    public void Dispose() {
        // Dispose the scope, which will also dispose the DbContext and other scoped services.
        _scope.Dispose();
    }
}