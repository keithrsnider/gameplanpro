using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Models;

[Table("players")]
public class Player : BaseEntity
{
	[Required]
	[MaxLength(100)]
	public required string LastName { get; set; }

	public int Number { get; set; }

	public int TeamId { get; set; }
	public Team Team { get; set; } = null!;

	public class Configuration : IEntityTypeConfiguration<Player>
	{
		public void Configure(EntityTypeBuilder<Player> builder)
		{
			ConfigureBase(builder);

			builder.HasOne(e => e.Team)
				.WithMany(t => t.Players)
				.HasForeignKey(e => e.TeamId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
