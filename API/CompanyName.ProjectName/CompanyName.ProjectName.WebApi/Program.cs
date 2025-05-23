using CompanyName.ProjectName.Mapping;
using CompanyName.ProjectName.WebApi.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// This method gets called by the runtime. Use this method to add services to the container.
// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
builder.Services.AddControllers(setupAction =>
{
    // Determines if a 406 response code (an unsupprted request response type) is returned
    // by the API when requested by the consumer.
    setupAction.ReturnHttpNotAcceptable = true;
})
.AddNewtonsoftJson(setupAction =>
{
    // For converting JSON values to Microsoft.AspNetCore.JsonPatch.JsonPatchDocument
    setupAction.SerializerSettings.ContractResolver =
       new CamelCasePropertyNamesContractResolver();
})
.AddXmlDataContractSerializerFormatters() // Adds the XML API response format, if requested. JSON is supported by default.
.ConfigureApiBehaviorOptions(setupAction =>
{
    setupAction.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Type = "modelvalidationproblem",
            Title = "One or more model validation errors occurred.",
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = "See the errors property for details.",
            Instance = context.HttpContext.Request.Path
        };

        problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);

        return new UnprocessableEntityObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

// New code: Register CORS policy for development mode only.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader();
    });
});

// For catching, logging and returning appropriate controller related errors
builder.Services.AddScoped<ApiExceptionFilter>();

// Register the shared dependencies in the Mapping project
DependencyConfig.Register(builder.Services, builder.Configuration, System.Reflection.Assembly.GetEntryAssembly().GetName().Name);
var app = builder.Build();

// Call the database seeding logic.
DatabaseConfig.SeedDatabases(app);

// Will create a file logger before the database exists
Log.Logger = LoggerConfig.CreateLogger();
Log.Information("Starting Up");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Allow cross origin resource sharing for testing all development requests.
    // This should not be done for a production build.
    app.UseCors("AllowAll");  // Updated: Using named policy "AllowAll" in development.
}
else
{
    // In a non-development environment, this adds middleware to the pipeline that
    // will catch exceptions, log them, and re-execute the request in an
    // alternate pipeline. The request will not be re-executed if the response has
    // already started.
    app.UseExceptionHandler(appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("There was an unexpected error.");
        });
    });
}

// Add logging to the request pipeline
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
LoggerConfig.Configure(loggerFactory);

// Adds middleware for redirecting HTTP requests to HTTPS
app.UseHttpsRedirection();

// Marks the position in the middleware pipeline where a routing
// decision is made (where an endpoint is selected)
app.UseRouting();

// Adds the Microsoft.AspNetCore.Authorization.AuthorizationMiddleware to the specified
// Microsoft.AspNetCore.Builder.IApplicationBuilder, which enables authorization
// capabilities.
app.UseAuthorization();

// Marks the position in the middleware pipeline where the selected
// endpoint is executed. Adds endpoints for our controller actions
// but no routes are specified. For a Web API, use attributes at controller
// and action level: [Route], [HttpGet], ...
app.MapControllers();

// Will create a database logger now that the database exists
Log.Logger = LoggerConfig.CreateLogger();

app.Run();

Log.CloseAndFlush();
