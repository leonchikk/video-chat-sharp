using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Net.WebSockets;
using System.Text;
using VoiceEngine.API.Middlewares;
using VoiceEngine.API.Sockets;

namespace VoiceEngine.API
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ConnectionsManager>();
            services.AddSingleton<SocketHandler>();
            services.AddMvc(options => options.EnableEndpointRouting = false);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            services.AddAuthentication()
                    .AddJwtBearer(cfg =>
                    {
                        cfg.RequireHttpsMetadata = false;
                        cfg.SaveToken = true;
                        cfg.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidIssuer = "Test",
                            ValidAudience = "Test",
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Please, don't you mind create smth more clever than this"))
                        };
                    });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();
            app.UseWebSockets();
            app.UseMiddleware<SocketMiddleware>();
            app.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
