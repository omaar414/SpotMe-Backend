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
    [Route("api/friendship")]
    public class FriendshipControllers : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FriendshipControllers(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Add")]
        [Authorize]
        public async Task<IActionResult> AddFriendship([FromBody] AddFriendDTO newFriend)
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

            //Search for the friend Username in the DB
            var friend = await _context.Users.FirstOrDefaultAsync(u => u.Username == newFriend.FriendUsername);

            // if friend is null that means the friend username dont exist in the DB
            if(friend == null) { return NotFound(new { message = "Username doesn't exist "}); }     

            // if user is trying to add himself as a friend , return bad request
            if(userIdInt == friend.Id) { return BadRequest(new{message="You can't add yourself" }); }

            

            //if friendship already exist between user and friend , return bad request
            bool friendshipExist = await _context.Friendships.AnyAsync(f => f.UserId == userIdInt && f.FriendId == friend.Id);
            
            if(friendshipExist)  { return BadRequest(new { message = "Users are already friends" }); }
            
            // We create the friendship
            var friendship = new Friendship {
                UserId = userIdInt,
                FriendId = friend.Id
            };

            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();
            
            return Ok(new {message="Friend added successfully"});
        }

        [HttpGet("Friends")]
        [Authorize]
        public async Task<IActionResult> GetFriends()
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

            //We obtain all the Friendships in the Db where the UserId match the Id of the user we have 
            var friends = await _context.Friendships
        .Where(f => f.UserId == userIdInt)
        .Select(f => new 
        {
            Username = f.Friend.Username,
            Latitude = f.Friend.Location != null ? f.Friend.Location.Latitude : (double?)null,
            Longitude = f.Friend.Location != null ? f.Friend.Location.Longitude : (double?)null,
            UpdatedAt = f.Friend.Location != null ? f.Friend.Location.UpdatedAt : (DateTime?)null
        })
        .ToListAsync();

            // if the list is equal to cero we send this message
            if(friends.Count == 0) { return Ok (new {message ="You don't have any friends"}); }

            return Ok(friends);
        }



        [HttpPost("Send-Request")]
        [Authorize]
    public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestDTO request)
    {
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

    // Buscar al usuario receptor por Username
    var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.ReceiverUsername);
    if (receiver == null)
        return NotFound(new { message = "User not found." });

    if (userIdInt == receiver.Id)
        return BadRequest(new { message = "You cannot send a friend request to yourself." });

    // Verificar si ya hay una solicitud pendiente
    var existingRequest = await _context.FriendRequests
        .FirstOrDefaultAsync(fr => fr.SenderId == userIdInt && fr.ReceiverId == receiver.Id && fr.Status == "Pending");

    if (existingRequest != null)
        return BadRequest(new { message = "Friend request already sent." });

    // Crear la solicitud de amistad
    var friendRequest = new FriendRequest
    {
        SenderId = userIdInt,
        ReceiverId = receiver.Id,
        Status = "Pending"
    };

    _context.FriendRequests.Add(friendRequest);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Friend request sent successfully!" });
}

[HttpPost("Accept-Request")]
[Authorize]
public async Task<IActionResult> AcceptFriendRequest([FromBody] HandleFriendRequestDTO request)
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim == null)
        return Unauthorized(new { message = "User not authenticated." });

    if (!int.TryParse(userIdClaim, out int userIdInt))
        return BadRequest(new { message = "Invalid User ID." });

    // Buscar la solicitud de amistad
    var friendRequest = await _context.FriendRequests
        .FirstOrDefaultAsync(fr => fr.Id == request.RequestId && fr.ReceiverId == userIdInt && fr.Status == "Pending");

    if (friendRequest == null)
        return NotFound(new { message = "Friend request not found." });

    // Crear la amistad en la tabla de Friendships
    var friendship = new Friendship
    {
        UserId = friendRequest.SenderId,
        FriendId = friendRequest.ReceiverId
    };

    _context.Friendships.Add(friendship);

    // Actualizar el estado de la solicitud de amistad a "Accepted"
    friendRequest.Status = "Accepted";
    await _context.SaveChangesAsync();

    return Ok(new { message = "Friend request accepted!" });
}

[HttpPost("Reject-Request")]
[Authorize]
public async Task<IActionResult> RejectFriendRequest([FromBody] HandleFriendRequestDTO request)
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim == null)
        return Unauthorized(new { message = "User not authenticated." });

    if (!int.TryParse(userIdClaim, out int userIdInt))
        return BadRequest(new { message = "Invalid User ID." });

    // Buscar la solicitud de amistad
    var friendRequest = await _context.FriendRequests
        .FirstOrDefaultAsync(fr => fr.Id == request.RequestId && fr.ReceiverId == userIdInt && fr.Status == "Pending");

    if (friendRequest == null)
        return NotFound(new { message = "Friend request not found." });

    // Actualizar el estado de la solicitud de amistad a "Rejected"
    friendRequest.Status = "Rejected";
    await _context.SaveChangesAsync();

    return Ok(new { message = "Friend request rejected!" });
}

[HttpGet("Pending-Requests")]
[Authorize]
public async Task<IActionResult> GetPendingFriendRequests()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim == null)
        return Unauthorized(new { message = "User not authenticated." });

    if (!int.TryParse(userIdClaim, out int userIdInt))
        return BadRequest(new { message = "Invalid User ID." });

    // Obtener todas las solicitudes de amistad pendientes donde el usuario es el receptor
    var pendingRequests = await _context.FriendRequests
        .Where(fr => fr.ReceiverId == userIdInt && fr.Status == "Pending")
        .Select(fr => new
        {
            SenderUsername = fr.Sender.Username, // Nombre del usuario que envió la solicitud
            SentAt = fr.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") // Fecha y hora en que se envió la solicitud
        })
        .ToListAsync();

        if(pendingRequests.Count == 0) {return Ok(new {message="You have no pending friend requests"});}

        return Ok(pendingRequests);
}


    }
}