using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StoreApi.Models;
using StoreApi.Services;

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
builder.Services.AddScoped<ImageUploadService>();


var app = builder.Build();

// Swagger 只在開發環境啟用
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthorization();

// 這行「一定要有」
app.MapControllers();

app.Run();

