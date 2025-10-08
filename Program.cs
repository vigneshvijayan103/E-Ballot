using Microsoft.Data.SqlClient;
using System.Data;
using EBallotApi.Services;

var builder = WebApplication.CreateBuilder(args);

// --- CORS setup ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500") //frontend connection
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


builder.Services.AddTransient<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IVoterService, VoterService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
