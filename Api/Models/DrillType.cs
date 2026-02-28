using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Models;

[Table("drill_types")]
public class DrillType : BaseEntity
{
	[Required]
	[MaxLength(50)]
	public required string Name { get; set; }

	public ICollection<Drill> Drills { get; set; } = [];
	public ICollection<PlanDrill> PlanDrills { get; set; } = [];

	public class Configuration : IEntityTypeConfiguration<DrillType>
	{
		public void Configure(EntityTypeBuilder<DrillType> builder)
		{
			ConfigureBase(builder);

			builder.HasData(
				new DrillType { Id = 1, Name = "Hitting" },
				new DrillType { Id = 2, Name = "Pitching" },
				new DrillType { Id = 3, Name = "Base Running" },
				new DrillType { Id = 4, Name = "Fielding" },
				new DrillType { Id = 5, Name = "Conditioning" },
				new DrillType { Id = 6, Name = "Warm-up" }
			);
		}
	}
}
