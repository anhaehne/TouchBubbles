using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HADotNet.Core.Clients;

namespace TouchBubbles.Shared.Services
{
    public interface IEntityService
    {
        Task<IReadOnlyCollection<string>> GetEntitiesAsync();
    }
}
