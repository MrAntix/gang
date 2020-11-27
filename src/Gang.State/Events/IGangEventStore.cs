using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Gang.State.Events
{
    public interface IGangEventStore
    {
        /// <summary>
        /// Commit Events to the store
        /// </summary>
        Task CommitAsync(
            IEnumerable<IGangEvent> events);

        /// <summary>
        /// Load Events for a given gang
        /// </summary>
        Task<IImmutableList<IGangEvent>> LoadAsync(
            string gangId, uint sequenceNumber);
    }
}
