using Gang.Management.Events;
using System;
using System.Threading.Tasks;

namespace Gang.Management
{
    public static class GangManagmentExtensions
    {
        public static IGangMember GetMember(
            this IGangController controller,
            byte[] memberId
            )
        {
            return controller.GetGang().MemberById(memberId);
        }

        public static Task DisconnectAsync(
            this IGangController controller,
            string memberId,
            string reason = null)
        {
            return controller.DisconnectAsync(memberId.GangToBytes(), reason);
        }

        public static decimal GetPercentage(
            this GangProgressState state)
        {
            return Math.Round(
                (decimal)state.Index / state.Count * 10000
                ) / 100;
        }

        public static TimeSpan GetEndEstimate(
            this GangProgressState state)
        {
            if (state.EndedOn.HasValue) return TimeSpan.Zero;

            var timeTaken = DateTimeOffset.Now - state.StartedOn;

            return timeTaken * state.Count / state.Index
                - timeTaken;
        }
    }
}
