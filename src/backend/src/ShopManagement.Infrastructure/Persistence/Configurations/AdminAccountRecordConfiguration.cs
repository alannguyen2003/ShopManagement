using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShopManagement.Infrastructure.Persistence.Configurations;

public sealed class AdminAccountRecordConfiguration : IEntityTypeConfiguration<AdminAccountRecord>
{
    public void Configure(EntityTypeBuilder<AdminAccountRecord> builder)
    {
        builder.ToTable("AdminAccounts", table => table.HasCheckConstraint("CK_AdminAccounts_SingleRow", "\"Id\" = 1"));

        builder.HasKey(record => record.Id);
        builder.Property(record => record.Id).ValueGeneratedNever();

        builder.Property(record => record.SignInName).IsRequired();
        builder.Property(record => record.PasswordAlgorithm).IsRequired();
        builder.Property(record => record.PasswordSalt).IsRequired();
        builder.Property(record => record.PasswordIterations).IsRequired();
        builder.Property(record => record.PasswordHash).IsRequired();
    }
}
