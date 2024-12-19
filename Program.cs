using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

// DbContext 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });



builder.Services.AddCors(options =>
{
    //options.AddPolicy("ReactApp", policyBuilder =>
    //{
    //    policyBuilder.WithOrigins("http://localhost:5173")
    //        .AllowAnyMethod()
    //        .AllowAnyHeader()
    //        .AllowCredentials();
    //});
    //options.AddPolicy("ViteApp", policyBuilder =>
    //{
    //    policyBuilder.WithOrigins("https://bytequiz-byteslasher.vercel.app")
    //        .AllowAnyMethod()
    //        .AllowAnyHeader()
    //        .AllowCredentials();
    //});

    options.AddPolicy("AllowSpecificOrigins", policyBuilder =>
    {
        policyBuilder.WithOrigins(
                "http://localhost:5173", // Local React dev server
                "https://bytequiz-byteslasher.vercel.app" // Deployed Vite frontend
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Enable credentials if using cookies
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

//app.UseCors("ReactApp");
//app.UseCors("ViteApp");

app.UseCors("AllowSpecificOrigins");

app.Run();
