using ToolInventory.API.Middleware;

namespace ToolInventory.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseToolInventoryPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseCors("FrontendClients");
        app.UseRateLimiter();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");
        return app;
    }
}
