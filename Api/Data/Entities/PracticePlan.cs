using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Models;

[Table("practice_plans")]
public class PracticePlan : BaseEntity
{
	[Required]
	[MaxLength(200)]
	public required string Name { get; set; }

	[MaxLength(200)]
	public string? Location { get; set; }

	public int? IntendedDuration { get; set; }

	[MaxLength(2000)]
	public string? Description { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

	public required string UserId { get; set; }
	public AppUser User { get; set; } = null!;

	public ICollection<Section> Sections { get; set; } = [];

	public class Configuration : IEntityTypeConfiguration<PracticePlan>
	{
		public void Configure(EntityTypeBuilder<PracticePlan> builder)
		{
			ConfigureBase(builder);

			builder.HasOne(e => e.User)
				.WithMany()
				.HasForeignKey(e => e.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
