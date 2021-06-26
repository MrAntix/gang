using Gang.Management.Events;
using System;

namespace Gang.Management
{
    public sealed class GangProgress
    {
        GangProgress(
            IGangManager gangManager, string gangId,
            GangProgressState state,
            int steps
            )
        {
            GangManager = gangManager;
            GangId = gangId;
            State = state;
            Steps = steps;
        }

        public IGangManager GangManager { get; }
        public string GangId { get; }
        public int Steps { get; }

        public int Step { get; private set; }
        public GangProgressState State { get; private set; }

        public static GangProgress Start(
            IGangManager gangManager, string gangId,
            string text, long count, int steps = 100
            )
        {
            return new GangProgress(
                gangManager, gangId,
                GangProgressState.Start(text, count),
                steps
                );
        }

        public void Increment(
            long by,
            string text = null
            )
        {
            var lastStep = Step;

            State = State.Increment(by, text);
            Step = (int)Math.Floor(
                (decimal)Steps * State.Index / State.Count
                );

            if (
                !string.IsNullOrWhiteSpace(text)
                || Step != lastStep
                )
                GangManager.RaiseEvent(State, GangId);
        }

        public void End(
            string text = null
            )
        {
            State = State.End(text);
            Step = Steps;

            GangManager.RaiseEvent(State, GangId);
        }
    }
}
