using NodeVerificationBot.Types;

namespace NodeVerificationBot.Interfaces
{
    public interface IRegisterRepository
    {
        RegisterData GetRegisteringUser(ulong discordId);
        void ClearRegisteringUser(ulong discordId);
        void CreateRegisteringUser(RegisterData data);
    }
}
