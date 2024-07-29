using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrderService.Configuration;
using OrderService.Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<RabbitMQOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddScoped<RabbitMqService>(sp =>
{
    var options = sp.GetRequiredService<IOptions<RabbitMQOptions>>().Value;
    var logger = sp.GetRequiredService<ILogger<RabbitMqService>>();
    return new RabbitMqService(options.HostName, options.UserName, options.Password, logger);
});

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program)); // Register AutoMapper
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations at startup
await ApplyMigrationsAsync(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Method to apply migrations with retry logic
async Task ApplyMigrationsAsync(WebApplication app)
{
    const int MAX_RETRIES = 5;
    const int DELAY_5_SECONDS_MILLI = 5000;
    const int DELAY_30_SECONDS_MILLI = 30000;

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var retries = 0;

        while (true)
        {
            try
            {
                if (retries == 0)
                {
                    await Task.Delay(DELAY_30_SECONDS_MILLI); //to give the User migration a chance to run from UserService. This is very crude
                }
                dbContext.Database.Migrate();
                break;
            }
            catch (Exception ex) when (retries < MAX_RETRIES)
            {
                retries++;
                Console.WriteLine($"Migration failed: {ex.Message}. Retrying in {DELAY_5_SECONDS_MILLI} ms...");
                await Task.Delay(DELAY_5_SECONDS_MILLI);
            }
        }

        if (retries == MAX_RETRIES)
        {
            throw new InvalidOperationException("Database migration failed after multiple retries.");
        }
    }
}
