using Infrastructure.Middlewares;

namespace Product.API.Extensions;

public static class ApplicationExtensions
{
    public static void UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json",
            "Swagger Product API v1"));
        app.UseMiddleware<ErrorWrappingMiddleware>();
        app.UseAuthentication();
        app.UseRouting();
        // app.UseHttpsRedirection(); //for Production only

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
    }
}