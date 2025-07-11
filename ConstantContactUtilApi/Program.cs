using ConstantContactUtilApi.EndPoints;
using Microsoft.Data.Sqlite;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Register endpoints from the extracted file
app.MapConstantContactEndpoints();

app.Run();