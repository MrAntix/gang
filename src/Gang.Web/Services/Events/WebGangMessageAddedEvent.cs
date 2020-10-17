namespace Gang.Web.Services.Events
{
    public class WebGangMessageAddedEvent
    {
        public WebGangMessageAddedEvent(
            string messageId,
            string text
            )
        {
            MessageId = messageId;
            Text = text;
        }

        public string MessageId { get; }
        public string Text { get; }
    }
}
