namespace DatabaseAdapters;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Repositories.SqLite;

public class UserConfiguration : IEntityTypeConfiguration<User> {

  public void Configure(EntityTypeBuilder<User> builder) {
    builder.ToTable("Users");
    builder.HasKey(u => u.Id);
  }
}
