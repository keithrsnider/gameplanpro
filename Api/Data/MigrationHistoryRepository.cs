using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace Api.Data;

// UseSnakeCaseNamingConvention() applies to all columns including the EF migrations
// history table, which breaks migrations. This restores the correct PascalCase column
// names for __EFMigrationsHistory only.
public class MigrationHistoryRepository(HistoryRepositoryDependencies dependencies)
    : NpgsqlHistoryRepository(dependencies)
{
    protected override void ConfigureTable(EntityTypeBuilder<HistoryRow> history)
    {
        base.ConfigureTable(history);
        history.Property(h => h.MigrationId).HasColumnName("MigrationId");
        history.Property(h => h.ProductVersion).HasColumnName("ProductVersion");
    }
}
