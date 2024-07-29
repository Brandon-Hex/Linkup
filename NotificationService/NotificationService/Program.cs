using Microsoft.Extensions.Options;
using UserService.Configuration;
using UserService.Services;
var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<RabbitMqService>(sp =>
{
    var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
    var logger = sp.GetRequiredService<ILogger<RabbitMqService>>();
    return new RabbitMqService(options.HostName, options.UserName, options.Password, logger);
});
builder.Services.AddSingleton<NotificationServiceListener>();

builder.Services.AddHttpClient();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Start the UserServiceListener after application starts
var rabbitMqService = app.Services.GetRequiredService<RabbitMqService>();
var userServiceListener = app.Services.GetRequiredService<NotificationServiceListener>();
userServiceListener.Start();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
