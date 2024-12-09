using AuthAPI.Context;
using AuthAPI.Map;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Adding CORS Origin because it will connect you to the frontend and access you the information and the request of te API 
//In summary, this CORS policy allows any external client (regardless of origin, method, or headers) to interact with the API, which is useful for testing or open APIs, but may need more restrictions in production environments for security reasons.
builder.Services.AddCors(option=>
{ 
option.AddPolicy("MyPolicy" , builder=>
{
    builder.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
});
});
// help you to create a configuration file and here we are giving a connection string that will connect ot the databse serve and create the database table 
// "SqlServerConnStr" this come from the appsetting.json which is the name of the connnection string 
builder.Services.AddDbContext<AddDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnStr"));
});

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("veryverysceretveryverysceretveryverysceret.....")),
        ValidateAudience = false,
        ValidateIssuer = false
    };
});

// Register Automapper 
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("MyPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
