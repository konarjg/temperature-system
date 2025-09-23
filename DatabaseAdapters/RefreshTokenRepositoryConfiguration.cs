namespace DatabaseAdapters;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Repositories.SqLite;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken> {

  public void Configure(EntityTypeBuilder<RefreshToken> builder) {
    builder.ToTable("RefreshTokens");
    builder.HasKey(r => r.Id);
    builder.HasOne(r => r.User).WithMany().HasForeignKey("UserId").IsRequired();
  }
}