using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Models;

[Table("sections")]
public class Section : BaseEntity
{
	[Required]
	[MaxLength(200)]
	public required string Name { get; set; }

	public int DisplayOrder { get; set; }

	public int PracticePlanId { get; set; }
	public PracticePlan PracticePlan { get; set; } = null!;

	public ICollection<PlanDrill> PlanDrills { get; set; } = [];

	public class Configuration : IEntityTypeConfiguration<Section>
	{
		public void Configure(EntityTypeBuilder<Section> builder)
		{
			ConfigureBase(builder);

			builder.HasOne(e => e.PracticePlan)
				.WithMany(pp => pp.Sections)
				.HasForeignKey(e => e.PracticePlanId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
