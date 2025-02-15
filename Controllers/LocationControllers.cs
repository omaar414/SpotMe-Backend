using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LocationTrackerAPI.Data;
using LocationTrackerAPI.Models;
using LocationTrackerAPI.Models.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocationTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/location")]
    public class LocationControllers : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationControllers(ApplicationDbContext context)
        {
            _context = context;
        }
       
       [HttpPost("Update")]
       [Authorize]
       public async Task<IActionResult> UpdateLocation([FromBody] LocationDTO userLocation)
       {
        // we get the user Id from the token
         var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

         if (userId == null)
            {
                return Unauthorized(new { message = "User not authentified." });
            }

            // Convertir userId in a int with name userIdInt
            if (!int.TryParse(userId, out int userIdInt))
            {
                return BadRequest(new { message = "Invalid User ID." });
            }

            // search if that userId have a location stored in DB
            var existingLocation = _context.Locations.FirstOrDefault(l => l.UserId == userIdInt);

            if (existingLocation == null)
            {
                // Si no tiene ubicación, crear una nueva
                var newLocation = new Location
                {
                    UserId = userIdInt,
                    Latitude = userLocation.Latitude,
                    Longitude = userLocation.Longitude,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Locations.Add(newLocation);
            }
            else
            {
                // Si ya existe, actualizar la ubicación
                existingLocation.Latitude = userLocation.Latitude;
                existingLocation.Longitude = userLocation.Longitude;
                existingLocation.UpdatedAt = DateTime.UtcNow;
            }

            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            return Ok(new { message = "Location updated successfully." });

       }

       [HttpGet]
       public async Task<IActionResult> GetLocation() // we obtain the location of the user and send it like a JSON file to the front end
       {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if(string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
        {
            return BadRequest(new { message = "Invalid User ID." });
        }

        var location = await _context.Locations.FirstOrDefaultAsync(l => l.UserId == userIdInt);

        if(location == null)
        {
            return NotFound(new{message = "Location not found"});
        }

        return Ok(new {
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            UpdatedAt = location.UpdatedAt.ToString("G")
        });
       }

       [HttpGet("All")]
       [Authorize]
       public async Task<IActionResult> GetAllLocations()
       {
        var locations = await _context.Locations        //we enter to the Locations table in the DB
            .Include(l => l.User)                       //we include the User informacion vincculated with every location
            .Select(l => new {                          //We select each one and return a new JSON object with the information we want
                username = l.User.Username,
                latitude = l.Latitude,
                longitude = l.Longitude,
                updatedAt = l.UpdatedAt.ToString("G")
            }).ToListAsync();
            return Ok(locations);                       //return the list of locations that have Locations in JSON format
       }



        
    }
}