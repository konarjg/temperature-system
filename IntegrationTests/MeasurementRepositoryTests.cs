using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseAdapters.Repositories;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests
{
    public class MeasurementRepositoryTests : BaseServiceTests
    {
        private readonly MeasurementRepository _repository;

        public MeasurementRepositoryTests()
        {
            _repository = new MeasurementRepository(DbContext);
        }

        [Fact]
        public async Task GetLatestAsync_ShouldReturnLatestMeasurement()
        {
            // Arrange
            var sensor = new Sensor { DisplayName = "Test Sensor", DeviceAddress = "Test Address" };
            DbContext.Sensors.Add(sensor);
            await DbContext.SaveChangesAsync();

            var oldMeasurement = new Measurement { TemperatureCelsius = 20, Timestamp = DateTime.UtcNow.AddDays(-1), SensorId = sensor.Id };
            var newMeasurement = new Measurement { TemperatureCelsius = 25, Timestamp = DateTime.UtcNow, SensorId = sensor.Id };
            DbContext.Measurements.AddRange(oldMeasurement, newMeasurement);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetLatestAsync(sensor.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newMeasurement.Id, result.Id);
        }
    }
}