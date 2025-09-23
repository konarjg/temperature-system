namespace DatabaseAdapters;

using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Repositories.SqLite;

public class VerificationTokenConfiguration : IEntityTypeConfiguration<VerificationToken> {

  public void Configure(EntityTypeBuilder<VerificationToken> builder) {
    builder.ToTable("VerificationTokens");
    builder.HasKey(v => v.Id);
    builder.HasOne(v => v.User).WithMany().HasForeignKey("UserId").IsRequired();
  }
}