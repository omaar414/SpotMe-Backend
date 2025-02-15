using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocationTrackerAPI.Models
{
    public class Location
    {
        public int Id { get; set; }
        public int UserId { get; set; } //Id of the User that the Location belongs to, this automatically make the relation with the User model. 
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public User? User { get; set; } //Navigation Property to access the unique User properties vinculated to this Location
    }
}