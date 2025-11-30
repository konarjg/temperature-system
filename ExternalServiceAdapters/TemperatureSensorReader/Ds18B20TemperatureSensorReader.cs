namespace ExternalServiceAdapters.TemperatureSensorReader;

using Domain.Entities;
using Domain.Services.External;
using Domain.Services.Interfaces;
using Iot.Device.OneWire;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Domain.Entities.Util;
using UnitsNet;

public class Ds18B20TemperatureSensorReader(
    ISensorService sensorService,
    ILogger<Ds18B20TemperatureSensorReader> logger) : ITemperatureSensorReader {
    
    private const string OneWireBusId = "w1";
    private const double PowerOnResetTemperature = 85.0;
    private const double Epsilon = 0.001;

    public async Task<List<Measurement>> ReadAsync() {
        List<Sensor> sensors = await sensorService.GetAllAsync();
        List<Measurement> measurements = new();

        foreach (Sensor sensor in sensors) {
            try {
                OneWireThermometerDevice device = new OneWireThermometerDevice(sensor.DeviceAddress, OneWireBusId);
                
                Temperature reading = await device.ReadTemperatureAsync();

                if (Math.Abs(reading.DegreesCelsius - PowerOnResetTemperature) < Epsilon)
                {
                    logger.LogWarning("Sensor at address {Address} returned a power-on-reset value of 85°C, indicating a read error.", sensor.DeviceAddress);
                    sensor.State = SensorState.Unavailable;
                    continue;
                }

                Measurement newMeasurement = new() {
                    Timestamp = DateTime.UtcNow,
                    TemperatureCelsius = (float)reading.DegreesCelsius,
                    SensorId = sensor.Id,
                    Sensor = sensor
                };
                
                sensor.State = SensorState.Operational;
                measurements.Add(newMeasurement);
            }
            catch (DirectoryNotFoundException ex) {
                logger.LogError(ex, "Failed to read from sensor {Address}. The 1-Wire bus or device directory was not found. Is the interface enabled?", sensor.DeviceAddress);
                sensor.State = SensorState.Unavailable;
            }
            catch (IOException ex) {
                logger.LogError(ex, "Failed to read from sensor {Address}. This may indicate it is disconnected.", sensor.DeviceAddress);
                sensor.State = SensorState.Unavailable;
            }
        }

        return measurements;
    }
}