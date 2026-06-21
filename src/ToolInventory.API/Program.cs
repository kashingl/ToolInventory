using Microsoft.EntityFrameworkCore;
using ToolInventory.API.Extensions;
using ToolInventory.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddToolInventorySwagger()
    .AddToolInventoryCore(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}

app.UseToolInventoryPipeline();
app.Run();

public partial class Program { }
