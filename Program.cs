using System.Text;
using LocationTrackerAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add Controllers
builder.Services.AddControllers();

//DataBase Connection 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString)); // Add DbContext

// Configurar JWT Authentication
var JwtKey = builder.Configuration["Jwt:Key"];
if(string.IsNullOrEmpty(JwtKey))
{
    throw new Exception("Key JWT not configurated in appsetting.json");
}

var key = Encoding.ASCII.GetBytes(JwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});


builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(builder =>
    builder.WithOrigins("http://localhost:5173") //permite solicitudes desde react
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.UseCors();
app.UseAuthentication(); // Habilita JWT
app.UseAuthorization();  // Habilita autorizaciones con `[Authorize]`

//Add MapControllers
app.MapControllers();

app.Run();


