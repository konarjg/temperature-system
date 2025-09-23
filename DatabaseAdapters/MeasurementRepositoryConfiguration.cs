namespace DatabaseAdapters;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Repositories.SqLite;

public class MeasurementConfiguration : IEntityTypeConfiguration<Measurement> {

  public void Configure(EntityTypeBuilder<Measurement> builder) {
    builder.ToTable("Measurements");
    builder.HasKey(m => m.Id);
    builder.HasOne(m => m.Sensor).WithMany().HasForeignKey(m => m.SensorId).IsRequired();
  }
}