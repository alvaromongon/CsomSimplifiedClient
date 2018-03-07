using System.Threading.Tasks;
using Microsoft.ProjectServer.Client;

namespace Csom.Client.Abstractions
{
    public interface IEntityTypeClient
    {
        Task<EntityTypes> GetAllAsync();        
    }
}
