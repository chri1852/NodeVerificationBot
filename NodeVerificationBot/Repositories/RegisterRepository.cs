using NodeVerificationBot.Interfaces;
using NodeVerificationBot.Types;
using System.Collections.Generic;

namespace NodeVerificationBot.Repositories
{
    public class RegisterRepository : IRegisterRepository
    {
        public Dictionary<ulong, RegisterData> _registeringUsers;

        public RegisterRepository()
        {
            _registeringUsers = new Dictionary<ulong, RegisterData>();
        }

        public RegisterData GetRegisteringUser(ulong discordId)
        {
            if (_registeringUsers.ContainsKey(discordId))
            {
                return _registeringUsers[discordId];
            }

            return null;
        }

        public void ClearRegisteringUser(ulong discordId)
        {
            if (_registeringUsers.ContainsKey(discordId))
            {
                _registeringUsers.Remove(discordId);
            }
        }

        public void CreateRegisteringUser(RegisterData data)
        {
            if(!_registeringUsers.ContainsKey(data.DiscordId))
            {
                _registeringUsers.Add(data.DiscordId, data);
            }
        }
    }
}
