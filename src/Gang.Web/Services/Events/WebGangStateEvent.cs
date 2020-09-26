namespace Gang.Web.Services.Events
{
    public class WebGangStateEvent
    {
        public WebGangStateEvent(
            GangMessageAudit audit)
        {
            Audit = audit;
        }

        public GangMessageAudit Audit { get; }
    }
}
