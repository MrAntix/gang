namespace Gang.Web.Services.Events
{
    public class WebGangMessageAddedEvent :
        WebGangStateEvent
    {
        public WebGangMessageAddedEvent(
            string messageId,
            string text,
            GangMessageAudit audit) : base(audit)
        {
            MessageId = messageId;
            Text = text;
        }

        public string MessageId { get; }
        public string Text { get; }
    }
}
