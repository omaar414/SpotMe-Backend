using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LocationTrackerAPI.Models;
using Microsoft.EntityFrameworkCore;



namespace LocationTrackerAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions <ApplicationDbContext> options) : base(options){}

        public DbSet<User> Users{ get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }

    }
}