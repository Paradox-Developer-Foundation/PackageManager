using Microsoft.EntityFrameworkCore;
using ParadoxPM.Server.Configurations;
using ParadoxPM.Server.Models;
using ParadoxPM.Server.Repositories;
using ZLogger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PackageContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Logging.ClearProviders();
builder.Logging.AddZLoggerConsole();

builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<IFileRepository>(_ => new FileRepository(AppConfigurations.FilesPath));

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PackageContext>();
    await db.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
