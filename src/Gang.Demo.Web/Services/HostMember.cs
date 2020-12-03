using Gang.Commands;
using Gang.Demo.Web.Properties;
using Gang.Demo.Web.Services.Commands;
using Gang.Demo.Web.Services.State;
using Gang.State;
using Gang.State.Commands;
using Gang.State.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Demo.Web.Services
{
    public sealed class HostMember :
        GangStateHost<HostState>
    {
        readonly SetSettings _setSettings;

        public HostMember(
            AuthSettings authSettings,
            IGangCommandExecutor<HostState> executor,
            IGangStateStore store) :
            base(executor, store)
        {
            _setSettings = new SetSettings(authSettings.Enabled);
        }

        protected override async Task OnMemberConnectAsync(
            GangAudit audit)
        {
            await SetState(State, audit);
            await Controller.SendCommandAsync(
                _setSettings,
                memberIds: new[] { audit.MemberId }
                );
        }

        protected override async Task OnMemberDisconnectAsync(
            GangAudit audit)
        {
            await SetState(State, audit);
        }

        protected override async Task<GangState<HostState>>
            OnStateAsync(GangState<HostState> state)
        {
            var members = Controller.GetGang().Members.Where(m => m.Auth != null).ToArray();
            var usersOnline = state.Data.Users
                .Where(u => !string.IsNullOrWhiteSpace(u.Name) && members.Any(m => m.Auth.Id == u.Id));

            foreach (var member in members)
            {
                var user = state.Data.Users
                    ?.FirstOrDefault(u => u.Id == member.Auth.Id);

                if (user == null)

                    await Controller.SendStateAsync(
                        new
                        {
                            messages = new[]
                            {
                                new Message("Welcome", "Enter your name to join the chat"),
                                new Message("About-Data", "Data is held in memory and will clear down after a period of inactivity")
                            },
                            users = Array.Empty<object>()
                        },
                        new[] { member.Id }
                    );

                else

                    await Controller.SendStateAsync(
                        new
                        {
                            messages = state.Data
                                .Messages.Concat(user?.Messages ?? ImmutableList<Message>.Empty),
                            users = state.Data.Users.Select(u => new
                            {
                                u.Id,
                                u.Name,
                                isOnline = usersOnline.Contains(u),
                                isCurrent = u.Id == user?.Id
                            })
                        },
                        new[] { member.Id }
                    );
            }

            Console.WriteLine($"State: {JsonConvert.SerializeObject(state, Formatting.Indented)}");

            return await base.OnStateAsync(state);
        }

        protected async override Task OnCommandAsync(
            byte[] bytes, GangAudit audit)
        {
            try
            {
                await base.OnCommandAsync(bytes, audit);

                await NotifyAsync(
                    new Notify("received"),
                    new[] { audit.MemberId },
                    audit.Sequence
                );
            }
            catch (Exception ex)
            {
                await NotifyAsync(
                    new Notify(
                        "error", ex.Message
                    ),
                    new[] { audit.MemberId },
                    audit.Sequence
                );
            }
        }

        protected override async Task OnCommandErrorAsync(
            GangState<HostState> result, GangAudit audit)
        {
            await NotifyAsync(
                new Notify(
                    "error", string.Join("\n", result.Errors)
                ),
                new[] { audit.MemberId },
                audit.Sequence
            );
        }

        public async Task NotifyAsync(
            Notify command,
            byte[][] memberIds,
            uint? inReplyToSequenceNumber = null
            )
        {
            await Controller.SendCommandAsync(
                command,
                memberIds,
                inReplyToSequenceNumber
                );
        }
    }
}
