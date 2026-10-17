using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShopManagement.Infrastructure.Persistence.Configurations;

public sealed class AdminAccountRowConfiguration : IEntityTypeConfiguration<AdminAccountRow>
{
    public void Configure(EntityTypeBuilder<AdminAccountRow> builder)
    {
        // Mapped to the table as a view so it is never part of a migration.
        builder.HasNoKey();
        builder.ToView("AdminAccounts");
    }
}
