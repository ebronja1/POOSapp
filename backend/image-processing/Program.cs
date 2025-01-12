using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure FormOptions to increase MultipartBodyLengthLimit (optional for file uploads)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10 MB
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder => 
    {
        // Adjust the URL to your frontend's origin (e.g., for local development)
        builder.WithOrigins("http://localhost:3000") // Frontend URL (replace with your frontend URL)
               .AllowAnyMethod() // Allow any HTTP method (GET, POST, etc.)
               .AllowAnyHeader(); // Allow any HTTP header
    });
});

var app = builder.Build();

// Use CORS middleware
app.UseCors("AllowFrontend");

// Use other middlewares
//app.UseHttpsRedirection();
//app.UseAuthorization();

// Map controllers
app.MapControllers();

// Run the application
app.Run();
