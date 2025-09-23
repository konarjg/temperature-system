namespace DatabaseAdapters;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Repositories.SqLite;

public class SensorConfiguration : IEntityTypeConfiguration<Sensor> {

  public void Configure(EntityTypeBuilder<Sensor> builder) {
    builder.ToTable("Sensors");
    builder.HasKey(s => s.Id);
  }
}
