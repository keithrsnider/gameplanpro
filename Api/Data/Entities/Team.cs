using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Models;

[Table("teams")]
public class Team : BaseEntity
{
	[Required]
	[MaxLength(200)]
	public required string Name { get; set; }

	public string UserId { get; set; } = null!;
	public AppUser User { get; set; } = null!;

	public ICollection<Coach> Coaches { get; set; } = [];
	public ICollection<Player> Players { get; set; } = [];

	public class Configuration : IEntityTypeConfiguration<Team>
	{
		public void Configure(EntityTypeBuilder<Team> builder)
		{
			ConfigureBase(builder);

			builder.HasOne(e => e.User)
				.WithOne()
				.HasForeignKey<Team>(e => e.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasIndex(e => e.UserId).IsUnique();
		}
	}
}
