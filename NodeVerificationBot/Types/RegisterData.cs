using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeVerificationBot.Types
{
    public class RegisterData
    {
        public ulong DiscordId { get; set; }
        public string UplandUsername { get; set; }
        public long PropId { get; set; }
        public string Address { get; set; }
        public int Price { get; set; }
    }
}
