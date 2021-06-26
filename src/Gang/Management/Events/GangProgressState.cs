using System;

namespace Gang.Management.Events
{
    public sealed class GangProgressState
    {
        public GangProgressState(
            string text,
            long count,
            long index,
            DateTimeOffset startedOn,
            DateTimeOffset? endedOn
            )
        {
            Text = text;
            StartedOn = startedOn;
            EndedOn = endedOn;
            Index = index;
            Count = count;
        }

        public string Text { get; }
        public DateTimeOffset StartedOn { get; }
        public long Index { get; }
        public DateTimeOffset? EndedOn { get; }
        public long Count { get; }

        public static GangProgressState Start(
            string text,
            long count
            )
        {
            return new GangProgressState(
                text,
                count, 0,
                DateTimeOffset.Now, null
                );
        }

        public GangProgressState Increment(
            long by,
            string text = null
            )
        {
            return new GangProgressState(
                text ?? Text,
                Count, Index + by,
                StartedOn, null
                );
        }

        public GangProgressState End(
            string text = null
            )
        {
            return new GangProgressState(
                text ?? Text,
                Count, Count,
                StartedOn, DateTimeOffset.Now
                );
        }
    }
}
