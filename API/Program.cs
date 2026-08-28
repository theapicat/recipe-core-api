using API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogLogging();
builder.Services.AddApplicationServices();
builder.Services.AddDatabaseServices(builder.Configuration);

var app = builder.Build();


app.UseSerilogRequestLogging();

app.UseRouting();

app.MapControllers();

// SignalR Hub mapping - matcher rutingen fra Gateway (/hubs/*)
// app.MapHub<RecipeHub>("/hubs/recipe");

app.Logger.LogInformation("🚀 Applikasjonen har startet og lytter på forespørsler!");
app.Run();