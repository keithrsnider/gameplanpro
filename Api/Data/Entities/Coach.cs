using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Models;

[Table("coaches")]
public class Coach : BaseEntity
{
	[Required]
	[MaxLength(100)]
	public required string Name { get; set; }

	public CoachType Type { get; set; }

	public int TeamId { get; set; }
	public Team Team { get; set; } = null!;

	public class Configuration : IEntityTypeConfiguration<Coach>
	{
		public void Configure(EntityTypeBuilder<Coach> builder)
		{
			ConfigureBase(builder);

			builder.Property(e => e.Type)
				.HasConversion<string>()
				.HasMaxLength(20)
				.IsRequired();

			builder.HasOne(e => e.Team)
				.WithMany(t => t.Coaches)
				.HasForeignKey(e => e.TeamId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
