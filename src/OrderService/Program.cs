using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderService.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console());

var conn = builder.Configuration.GetConnectionString("Postgres") ??
           builder.Configuration["ConnectionStrings:Postgres"] ??
           "Host=postgres_order;Port=5432;Database=ordersdb;Username=postgres;Password=postgrespw";

builder.Services.AddDbContext<OrderDbContext>(opt => opt.UseNpgsql(conn));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger(); app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.EnsureCreated();
}

app.MapControllers();
app.Run();
