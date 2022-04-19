using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services;
using HolcombeScores.Api.Services.Adapters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace HolcombeScores.Api
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddSwaggerGen();

            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            services.AddScoped<IAccessRepository, AccessRepository>();
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<ITableServiceClientFactory, TableServiceClientFactory>();

            services.AddScoped<INewGameDtoAdapter, NewGameDtoAdapter>();
            services.AddSingleton<IGameDtoAdapter, GameDtoAdapter>();
            services.AddSingleton<IGamePlayerDtoAdapter, GamePlayerDtoAdapter>();
            services.AddSingleton<IGoalDtoAdapter, GoalDtoAdapter>();
            services.AddSingleton<IPlayerDtoAdapter, PlayerDtoAdapter>();
            services.AddSingleton<IAccessDtoAdapter, AccessDtoAdapter>();
            services.AddSingleton<IAccessRequestDtoAdapter, AccessRequestDtoAdapter>();
            services.AddSingleton<IAccessRequestedDtoAdapter, AccessRequestedDtoAdapter>();
            services.AddSingleton<IRecoverAccessDtoAdapter, RecoverAccessDtoAdapter>();
            services.AddSingleton<IMyAccessDtoAdapter, MyAccessDtoAdapter>();
            services.AddSingleton<ITeamDtoAdapter, TeamDtoAdapter>();

            services.AddScoped<IGameService, GameService>();
            services.AddScoped<IPlayerService, PlayerService>();
            services.AddScoped<IAccessService, AccessService>();
            services.AddScoped<ITeamService, TeamService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
