using API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogLogging();

builder.Services.AddApplicationServices();
builder.Services.AddMassTransitServices(builder.Configuration);

builder.Services.AddControllers();


var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseRouting();

app.MapControllers();

app.Logger.LogInformation("🚀 Applikasjonen har startet og lytter på forespørsler!");
app.Run();