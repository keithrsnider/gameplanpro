using Api.Repositories;
using Api.Services;

namespace Api.Extensions;

public static class ApplicationServiceExtensions
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services)
	{
		// Repositories
		services.AddScoped<IDrillTypeRepository, DrillTypeRepository>();
		services.AddScoped<IDrillRepository, DrillRepository>();
		services.AddScoped<IPracticePlanRepository, PracticePlanRepository>();
		services.AddScoped<ISectionRepository, SectionRepository>();
		services.AddScoped<IPlanDrillRepository, PlanDrillRepository>();
		services.AddScoped<ITeamRepository, TeamRepository>();
		// Infrastructure
		services.AddHttpContextAccessor();
		services.AddScoped<IUserContext, UserContext>();

		// Services
		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<IDrillTypeService, DrillTypeService>();
		services.AddScoped<IDrillService, DrillService>();
		services.AddScoped<IPracticePlanService, PracticePlanService>();
		services.AddScoped<ISectionService, SectionService>();
		services.AddScoped<IPlanDrillService, PlanDrillService>();
		services.AddScoped<ITeamService, TeamService>();
		services.AddScoped<ICoachService, CoachService>();

		return services;
	}
}
