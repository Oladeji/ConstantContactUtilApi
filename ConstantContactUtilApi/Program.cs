using SQLitePCL;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
string[] origins = { "http://localhost:5176/", "http://localhost:5176" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsConstants", builder =>
    {
        builder.AllowAnyMethod()
            .AllowAnyHeader()
            .WithOrigins(origins)
            .AllowCredentials();
    });
});
Batteries.Init();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("CorsConstants");
// Register endpoints from the extracted file
app.MapConstantContactEndpoints();

app.Run();