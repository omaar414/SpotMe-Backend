using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocationTrackerAPI.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }
        public int SenderId { get; set; }  // Usuario que envió la solicitud
        public User? Sender { get; set; }

        public int ReceiverId { get; set; } // Usuario que recibe la solicitud
        public User? Receiver { get; set; }

        public string Status { get; set; } = "Pending"; // Estado: Pending, Accepted, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Fecha de la solicitud
    }
}