using Server.Accessors;
using Server.Managers;
using Server.Middlewares;

namespace Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddSingleton<SessionManager>();
        builder.Services.AddScoped<ISessionAccessor, SessionAccessor>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.UseMiddleware<SessionMiddleware>();

        app.MapControllers();

        app.Run();
    }
}
