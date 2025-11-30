namespace ExternalServiceAdapters.TemperatureSensorReader;

using Domain.Entities;
using Domain.Services.External;
using Domain.Services.Interfaces;
using Iot.Device.OneWire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Domain.Entities.Util;
using UnitsNet;

public class Ds18B20TemperatureSensorReader(ILogger<Ds18B20TemperatureSensorReader> logger) : ITemperatureSensorReader {
    
    private const string OneWireBusId = "w1";
    private const double PowerOnResetTemperature = 85.0;
    private const double Epsilon = 0.001;

    public event ITemperatureSensorReader.OnMeasurementPerformed? MeasurementPerformed;
    public event ITemperatureSensorReader.OnSensorStateChanged? SensorStateChanged;

    public async Task<List<Measurement>> ReadAsync(List<Sensor> sensors) {
        List<Measurement> measurements = new();

        foreach (Sensor sensor in sensors) {
            try {
                OneWireThermometerDevice device = new(sensor.DeviceAddress, OneWireBusId);
                
                Temperature reading = await device.ReadTemperatureAsync();

                if (Math.Abs(reading.DegreesCelsius - PowerOnResetTemperature) < Epsilon) {
                    logger.LogWarning("Sensor at address {Address} returned a power-on-reset value of 85°C.", sensor.DeviceAddress);
                    sensor.State = SensorState.Unavailable;
                    SensorStateChanged?.Invoke(sensor);
                    continue;
                }

                Measurement newMeasurement = new Measurement {
                    Timestamp = DateTime.UtcNow,
                    TemperatureCelsius = (float)reading.DegreesCelsius,
                    SensorId = sensor.Id,
                    Sensor = sensor
                };
                
                sensor.State = SensorState.Operational;
                SensorStateChanged?.Invoke(sensor);
                MeasurementPerformed?.Invoke(newMeasurement);
                measurements.Add(newMeasurement);
            } catch (DirectoryNotFoundException) {
                logger.LogWarning("Sensor {Address} not found on bus. Check connection.", sensor.DeviceAddress);
                sensor.State = SensorState.Unavailable;
                SensorStateChanged?.Invoke(sensor);
            } catch (IOException) {
                logger.LogWarning("Failed to read from sensor {Address}. CRC check failed or device disconnected.", sensor.DeviceAddress);
                sensor.State = SensorState.Unavailable;
                SensorStateChanged?.Invoke(sensor);
            } catch (Exception ex) {
                logger.LogError(ex, "Unexpected error reading sensor {Address}.", sensor.DeviceAddress);
                sensor.State = SensorState.Unavailable;
                SensorStateChanged?.Invoke(sensor);
            }
        }

        return measurements;
    }
}