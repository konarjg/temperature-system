using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseAdapters
{
    public class VerificationTokenRepositoryConfiguration : IEntityTypeConfiguration<VerificationToken>
    {
        public void Configure(EntityTypeBuilder<VerificationToken> builder)
        {
            builder.ToTable("VerificationTokens");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Token).IsRequired();
            builder.Property(t => t.Expires).IsRequired();
            builder.HasOne(t => t.User).WithMany().IsRequired();
        }
    }
}