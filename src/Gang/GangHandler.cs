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

        async Task IGangHandler.HandleAsync(
            GangParameters parameters, IGangMember gangMember)
        {
            var gang = _gangs.AddMember(parameters.GangId, gangMember);
            await gangMember.SendAsync(
                gang.Host == gangMember ? GangMessageTypes.Host : GangMessageTypes.Member,
                gangMember.Id);

            try
            {
                while (gangMember.IsOpen)
                {
                    var result = await gangMember.ReceiveAsync();
                    if (result != null)
                    {
                        gang = _gangs[parameters.GangId];
                        if (gangMember == gang.Host)
                        {
                            var tasks = gang.Members
                                .Select(member => member.SendAsync(GangMessageTypes.State, result))
                                .ToArray();

                            await Task.WhenAll(tasks);
                        }
                        else
                        {
                            await gang.Host.SendAsync(GangMessageTypes.Command, result);
                        }
                    }
                }
            }
            catch (WebSocketException)
            {
            }

            gang = _gangs.RemoveMember(
                 parameters.GangId, gangMember);
            if (gang != null) await gang.Host.SendAsync(GangMessageTypes.Disconnect, gangMember.Id);
        }
    }
}