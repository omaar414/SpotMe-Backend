using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocationTrackerAPI.Data;
using Microsoft.AspNetCore.Mvc;
using LocationTrackerAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using LocationTrackerAPI.Models.DTOS;
using Microsoft.AspNetCore.Authorization;


namespace LocationTrackerAPI.Controllers
{   
    [ApiController]
    [Route("api/auth")]
    public class AuthControllers : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        public AuthControllers(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("Test")]
        public IActionResult Test()
        {
            return Ok(new {message = "AuthController Funcionando"});
        }

        
        
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO userDto) 
        {
            if(!ModelState.IsValid) // We check if the Required fileds of the model are all valid
            {
                return BadRequest(ModelState);
            }
            
            //We search if the username already exist and return an error if it does exist in the DB
            if( await _context.Users.AnyAsync(u => u.Username == userDto.Username) )
            {
                return BadRequest(new {message = "User already exist"});
            }
            
            //Encrypt the user register password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            
            //Create new User thta is going to be added to the DB
            var user = new User 
            {
                Username = userDto.Username,
                PasswordHash = hashedPassword

            };

            // Save the user in the DB
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new {message = "User created successfully"});


        }
        
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if(!ModelState.IsValid) // We check if the Required fileds of the model are all valid
            {
                return BadRequest(ModelState);
            }

            //We search for the user if the username exist in the DB
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            //If an User is found we verify the loginUser pasword with the DB user PasswordHash if veryf is false return an Unauthorized
            if(user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new {message = "Invalid username or password"});
            }

            //Generate the user Token
            var token = GenerateToken(user);
            return Ok(new {token});
        }

        // GenerateToken Method
        private string GenerateToken(User user)
        {
            // var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]); // obtain key from appsettings.json
            var jwtKey = _config["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key", "JWT Key is missing in appsettings.json");
            var key = Encoding.ASCII.GetBytes(jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7), // Valid token for 7 days
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [Authorize] // this mean thta you required a valid Token to enter or view this endpoint
        [HttpGet("ProtectedTest")]
        public IActionResult ProtectedTest()
        {
            return Ok(new {message = "Este endpoint esta protegido"});
        }
        
        
    }
}