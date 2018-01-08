using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gang.Tests
{
    public class FakeGangMember : IGangMember
    {
        Func<Task<byte[]>> _receiveActionAsync;

        public FakeGangMember(string id, int delay = 50)
        {
            Id = Encoding.UTF8.GetBytes(id);
            Sent = new List<Tuple<GangMessageTypes, byte[]>>();
            IsOpen = true;

            _receiveActionAsync = async () =>
            {
                await Task.Delay(delay);
                IsOpen = false;

                return null;
            };
        }

        public byte[] Id { get; }

        public bool IsOpen { get; set; }

        public void OnReceiveAction(Func<Task<byte[]>> actionAsync)
        {
            _receiveActionAsync = actionAsync;
        }

        async Task<byte[]> IGangMember.ReceiveAsync()
        {
            return _receiveActionAsync == null
                 ? null
                 : await _receiveActionAsync();
        }

        Task IGangMember.SendAsync(GangMessageTypes type, byte[] message)
        {
            Sent.Add(Tuple.Create(type, message));
            return Task.CompletedTask;
        }

        public IList<Tuple<GangMessageTypes, byte[]>> Sent { get; }
    }
}
