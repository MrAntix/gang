using Gang.Contracts;
using Gang.Serialization;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Gang
{

    public class GangHandler :
        IGangHandler
    {
        static object lockObject = new object();
        readonly ISerializationService _serializer;

        GangCollection _gangs;

        public GangHandler(
            ISerializationService serializer,
            GangCollection gangs)
        {
            _serializer = serializer;
            _gangs = gangs;
        }

        Gang IGangHandler.GangById(
            string gangId)
        {
            return _gangs[gangId];
        }

        async Task IGangHandler.HandleAsync(
            GangParameters parameters, IGangMember gangMember)
        {
            _gangs.AddMemberToGang(parameters.GangId, gangMember);

            var gang = _gangs[parameters.GangId];
            await gangMember.SendAsync(
                gang.HostMember == gangMember ? GangMessageTypes.Host : GangMessageTypes.Member,
                gangMember.Id);

            try
            {
                while (gangMember.IsConnected)
                {
                    var result = await gangMember.ReceiveAsync();
                    if (result != null)
                    {
                        gang = _gangs[parameters.GangId];
                        if (gangMember == gang.HostMember)
                        {
                            var tasks = gang.OtherMembers
                                .Select(member => member.SendAsync(GangMessageTypes.State, result))
                                .ToArray();

                            await Task.WhenAll(tasks);
                        }
                        else
                        {
                            await gang.HostMember.SendAsync(GangMessageTypes.Command, result);
                        }
                    }
                }
            }
            catch (WebSocketException)
            {
            }

            _gangs.RemoveMemberFromGang(parameters.GangId, gangMember);

            gang = _gangs[parameters.GangId];
            if (gang != null) await gang.HostMember.SendAsync(GangMessageTypes.Disconnect, gangMember.Id);
        }
    }
}