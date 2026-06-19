using ToolInventory.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddToolInventorySwagger()
    .AddToolInventoryCore(builder.Configuration);

var app = builder.Build();

app.UseToolInventoryPipeline();
app.Run();

public partial class Program { }
