using System;
using System.Threading.Tasks;
using DatabaseAdapters.Repositories;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests
{
    public class SensorRepositoryTests : BaseServiceTests
    {
        private readonly SensorRepository _repository;

        public SensorRepositoryTests()
        {
            _repository = new SensorRepository(DbContext);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllSensors()
        {
            // Arrange
            var sensor1 = new Sensor { DisplayName = "Sensor 1", DeviceAddress = "Address 1" };
            var sensor2 = new Sensor { DisplayName = "Sensor 2", DeviceAddress = "Address 2" };
            DbContext.Sensors.AddRange(sensor1, sensor2);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }
    }
}