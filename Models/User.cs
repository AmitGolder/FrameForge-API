using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameForge.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
