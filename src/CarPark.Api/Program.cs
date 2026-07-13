using CarPark.Api.Data;
using CarPark.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CarPark.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var connectionString = builder.Configuration.GetConnectionString("CarPark")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'CarPark' was not found.");

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IParkingChargeCalculator, ParkingChargeCalculator>();
        builder.Services.AddScoped<ICarParkService, CarParkService>();

        builder.Services.AddDbContext<CarParkDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.PropertyNamingPolicy = null);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<CarParkDbContext>();

            dbContext.Database.Migrate();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}