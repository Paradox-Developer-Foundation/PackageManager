using ParadoxPM.Server.Configurations;
using ParadoxPM.Server.Models;
using ParadoxPM.Server.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PackageContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

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

    // 添加示例数据（如果数据库为空）
    if (!await db.Packages.AnyAsync())
    {
        Console.WriteLine("添加初始数据...");
        var initialPackages = new List<Package>
        {
            new Package
            {
                Name = "Example Package",
                NormalizedName = "examplepackage",
                Version = "0.1",
                Description = "Example Package.",
                License = "MIT",
                Size = 876,
                Sha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                UploadDate = DateTime.UtcNow,
                IsActive = true,
                DownloadCount = 0,
                FilePath = "1_examplepackage-0.1.zip",
            },
        };

        await db.Packages.AddRangeAsync(initialPackages);
        await db.SaveChangesAsync();
        Console.WriteLine($"添加了 {initialPackages.Count} 个初始包");
    }
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
