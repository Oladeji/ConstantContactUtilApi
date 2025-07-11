

//using Microsoft.AspNetCore.Cors.Infrastructure;
//using Microsoft.Data.Sqlite;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//var dbPath = "Data Source=tokenstore.db";
//using (var connection = new SqliteConnection(dbPath))
//{
//    connection.Open();
//    var tableCmd = connection.CreateCommand();
//    tableCmd.CommandText = @"
//        CREATE TABLE IF NOT EXISTS Tokens (
//            UserId TEXT PRIMARY KEY,
//            AccessToken TEXT NOT NULL,
//            RefreshToken TEXT,
//            Expiry INTEGER
//        );";
//    tableCmd.ExecuteNonQuery();
//}


//builder.Services.AddControllers();
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//string[] origins = ["http://localhost:5176/", "http://localhost:5176"];
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("CorsConstants", builder =>
//    {
//        builder.AllowAnyMethod()
//            .AllowAnyHeader()
//            .WithOrigins(origins)
//            .AllowCredentials();
//    });
//});
//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();



////if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}



//app.UseCors("CorsConstants");


//app.MapControllers();

//app.Run();
