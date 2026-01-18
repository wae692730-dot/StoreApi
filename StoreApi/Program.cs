using Microsoft.EntityFrameworkCore;
using StoreApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 加入 API Controller
builder.Services.AddControllers();

// 加入 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<StoreDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("StoreDbContext")
    );
});

var app = builder.Build();

// Swagger 只在開發環境啟用
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// 這行「一定要有」
app.MapControllers();

app.Run();

