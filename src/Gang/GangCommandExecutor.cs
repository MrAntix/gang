using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Gang
{
    public sealed class GangCommandExecutor
    {
        readonly IImmutableDictionary<string, Func<JToken, GangMessageAudit, Task>> _handlers;

        GangCommandExecutor(
            IImmutableDictionary<string, Func<JToken, GangMessageAudit, Task>> handlers)
        {
            _handlers = handlers;
        }

        public static GangCommandExecutor Create()
        {
            return new GangCommandExecutor(
                ImmutableDictionary<string, Func<JToken, GangMessageAudit, Task>>.Empty);
        }

        public GangCommandExecutor Register<TCommand>(
            string type, Func<TCommand, GangMessageAudit, Task> handler)
        {
            return new GangCommandExecutor(
                _handlers.Add(type,
                    (j, a) => handler(j.ToObject<TCommand>(), a)
                ));
        }

        public Task ExecuteAsync(
            string type, JToken command, GangMessageAudit audit)
        {
            return _handlers[type](command, audit);
        }
    }
}