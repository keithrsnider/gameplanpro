using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Models;

[Table("plan_drills")]
public class PlanDrill : BaseEntity
{
	[Required]
	[MaxLength(200)]
	public required string Name { get; set; }

	public int Duration { get; set; }

	[MaxLength(5000)]
	public string? Instructions { get; set; }

	[MaxLength(500)]
	public string? DemoLink { get; set; }

	[MaxLength(100)]
	public string? CoachAssignment { get; set; }

	public int? PlayerCount { get; set; }
	public Guid? StationGroup { get; set; }
	public int DisplayOrder { get; set; }

	public int SectionId { get; set; }
	public Section Section { get; set; } = null!;

	public int? DrillTypeId { get; set; }
	public DrillType? DrillType { get; set; }

	public int? DrillId { get; set; }
	public Drill? Drill { get; set; }

	public class Configuration : IEntityTypeConfiguration<PlanDrill>
	{
		public void Configure(EntityTypeBuilder<PlanDrill> builder)
		{
			ConfigureBase(builder);

			builder.HasOne(e => e.Section)
				.WithMany(s => s.PlanDrills)
				.HasForeignKey(e => e.SectionId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(e => e.DrillType)
				.WithMany(dt => dt.PlanDrills)
				.HasForeignKey(e => e.DrillTypeId)
				.OnDelete(DeleteBehavior.SetNull);

			builder.HasOne(e => e.Drill)
				.WithMany(d => d.PlanDrills)
				.HasForeignKey(e => e.DrillId)
				.OnDelete(DeleteBehavior.SetNull);
		}
	}
}
