using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocationTrackerAPI.Models.DTOS
{
    public class SendFriendRequestDTO
    {
        public string ReceiverUsername { get; set; } = string.Empty;
    }
}