using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Models;

[Table("drills")]
public class Drill : BaseEntity
{
	[Required]
	[MaxLength(200)]
	public required string Name { get; set; }

	[MaxLength(2000)]
	public string? Description { get; set; }

	public int Duration { get; set; }

	[MaxLength(5000)]
	public string? Instructions { get; set; }

	[MaxLength(500)]
	public string? DemoLink { get; set; }

	public int? PlayerCount { get; set; }

	public DrillSource Source { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	public int DrillTypeId { get; set; }
	public DrillType DrillType { get; set; } = null!;

	public int? CoachId { get; set; }
	public Coach? Coach { get; set; }

	public string? UserId { get; set; }
	public AppUser? User { get; set; }

	public ICollection<PlanDrill> PlanDrills { get; set; } = [];

	public class Configuration : IEntityTypeConfiguration<Drill>
	{
		public void Configure(EntityTypeBuilder<Drill> builder)
		{
			ConfigureBase(builder);

			builder.Property(e => e.Source)
				.HasConversion<string>()
				.HasMaxLength(20)
				.IsRequired();

			builder.HasOne(e => e.DrillType)
				.WithMany(dt => dt.Drills)
				.HasForeignKey(e => e.DrillTypeId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.Coach)
				.WithMany()
				.HasForeignKey(e => e.CoachId)
				.OnDelete(DeleteBehavior.SetNull);

			builder.HasOne(e => e.User)
				.WithMany()
				.HasForeignKey(e => e.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
