using NodeVerificationBot.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NodeVerificationBot.Interfaces
{
    public interface IUplandApiRepository
    {
        Task<UplandProperty> GetPropertyById(long propertyId);
        Task<List<UplandAuthProperty>> GetPropertysByUsername(string username);
    }
}
