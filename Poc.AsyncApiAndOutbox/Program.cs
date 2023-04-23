using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Poc.AsyncApiAndOutbox.Features;
using Poc.AsyncApiAndOutbox.Outbox;
using Poc.AsyncApiAndOutbox.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OutboxContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddSingleton<ITimeProvider, TimeProvider>();
builder.Services.AddScoped<ServiceOne>();
builder.Services.AddScoped<ServiceTwo>();

builder.Services.AddOptions<OperationRequestOptions>()
    .Bind(builder.Configuration.GetSection(nameof(OperationRequestOptions)))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<OperationRequestService>();
builder.Services.AddScoped<OutboxService>();
builder.Services.AddHostedService<ServiceTwoHostedService>();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
