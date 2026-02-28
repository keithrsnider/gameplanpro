using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Models;

public abstract class BaseEntity
{
	public int Id { get; set; }
	public Guid Key { get; set; } = Guid.NewGuid();

	public static void ConfigureBase<T>(EntityTypeBuilder<T> builder) where T : BaseEntity
	{
		builder.HasKey(e => e.Id);
		builder.Property(e => e.Id).ValueGeneratedOnAdd();
		builder.HasIndex(e => e.Key).IsUnique();
		builder.Property(e => e.Key).HasDefaultValueSql("gen_random_uuid()");
	}
}
