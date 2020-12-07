using Gang.Management;
using Gang.Management.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Gang.Tests.Management.Fakes
{
    public sealed class FakeGangManager : IGangManager
    {
        uint _version;
        readonly Subject<IGangManagerEvent> _events = new();
        public IObservable<IGangManagerEvent> Events => _events;
        IImmutableList<ReceivedArgs> _received = ImmutableList<ReceivedArgs>.Empty;
        IImmutableList<SentArgs> _sent = ImmutableList<SentArgs>.Empty;

        public IImmutableList<ReceivedArgs> Received => _received;
        public IImmutableList<SentArgs> Sent => _sent;

        public void RaiseEvent<TEventData>(
            TEventData data,
            string gangId,
            byte[] memberId = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            lock (_events)
            {
                _events.OnNext(
                    new GangManagerEvent<TEventData>(
                        data,
                        new GangAudit(gangId, ++_version, memberId)
                    ));
            }
        }

        public GangMemberCollection GangById(string gangId)
        {
            return null;
        }

        public Task<GangMemberConnectionState> ManageAsync(
            GangParameters parameters,
            IGangMember gangMember)
        {
            var state = new GangMemberConnectionState();
            state.SetDisconnected();

            gangMember.ConnectAsync(
                new GangController(
                    this,
                    parameters.GangId, gangMember,
                    Receive,
                    Send,
                    null
                    ),
                () => Task.CompletedTask);

            return Task.FromResult(
                state
                );
        }

        Task Receive(byte[] data)
        {
            _received = _received.Add(new ReceivedArgs(data));
            return Task.CompletedTask;
        }

        Task Send(GangMessageTypes? type, byte[] data, GangAudit audit, IEnumerable<byte[]> memberIds)
        {
            _sent = _sent.Add(new SentArgs(type, data, audit, memberIds));
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }

        public sealed class ReceivedArgs
        {
            public ReceivedArgs(
                byte[] data
                )
            {
                Data = data?.ToImmutableList();
            }

            public IImmutableList<byte> Data { get; }
        }

        public sealed class SentArgs
        {
            public SentArgs(
                GangMessageTypes? type,
                byte[] data,
                GangAudit audit,
                IEnumerable<byte[]> memberIds = null
                )
            {
                Type = type;
                Data = data?.ToImmutableList();
                Audit = audit;
                MemberIds = memberIds?.ToImmutableList();
            }

            public GangMessageTypes? Type { get; }
            public IImmutableList<byte> Data { get; }
            public GangAudit Audit { get; }
            public IImmutableList<byte[]> MemberIds { get; }
        }
    }
}
