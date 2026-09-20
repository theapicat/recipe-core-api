using API.Extensions;
using Application.Extensions;
using Persistence.Extensions;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogLogging();

builder.Configuration.MigrateDatabase();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddExceptionHandling();

// Enum-verdier (f.eks. status på ubekreftede ingredienser) serialiseres som tekst, ikke tall.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));


var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRealtimeHubs();

app.Logger.LogInformation("🚀 Applikasjonen har startet og lytter på forespørsler!");
app.Run();