using Gang.Management.Events;
using System;
using System.Threading.Tasks;

namespace Gang.Management
{
    public interface IGangManager : IDisposable
    {
        /// <summary>
        /// Manage a gang member given the parameters
        /// </summary>
        /// <param name="parameters">Gang Id and Token</param>
        /// <param name="gangMember">Gang Member</param>
        /// <returns>Connection State</returns>
        Task<GangMemberConnectionState> ManageAsync(
            GangParameters parameters, IGangMember gangMember);

        /// <summary>
        /// Observable events
        /// </summary>
        IObservable<IGangManagerEvent> Events { get; }

        /// <summary>
        /// Raise a gang event
        /// </summary>
        /// <typeparam name="TEventData">Event Data Type</typeparam>
        /// <param name="data">Event data</param>
        /// <param name="gangId">Gang Id, required if memberId is passed</param>
        /// <param name="memberId">Member Id</param>
        void RaiseEvent<TEventData>(TEventData data,
            string gangId = null, byte[] memberId = null);

        /// <summary>
        /// Get running a gang by id
        /// </summary>
        /// <param name="gangId">Gang Id</param>
        /// <returns>Gang</returns>
        GangMemberCollection GangById(string gangId);
    }
}