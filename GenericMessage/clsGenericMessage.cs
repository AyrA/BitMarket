using System.Collections.Specialized;

namespace GenericHandler
{
    public class GenericMessage
    {
        /// <summary>
        /// Sender Address
        /// </summary>
        public string Sender
        { get; set; }

        /// <summary>
        /// Receiver Address
        /// </summary>
        public string Receiver
        { get; set; }

        /// <summary>
        /// Command (Subject)
        /// </summary>
        public string Command
        { get; set; }

        /// <summary>
        /// Content (body)
        /// </summary>
        public string RawContent
        { get; set; }

        /// <summary>
        /// Custom Data for Server
        /// </summary>
        public object Tag
        { get; set; }

        /// <summary>
        /// Server that generated the message (and should be used for answering it)
        /// </summary>
        public GenericServer Server
        { get; set; }

        /// <summary>
        /// Creates an empty Message
        /// </summary>
        public GenericMessage()
        {
            Sender = Receiver = Command = RawContent = null;
            Tag = null;
            Server = null;
        }

        /// <summary>
        /// Creates an exact, independant copy of this Message
        /// </summary>
        /// <returns>Cloned Message</returns>
        public GenericMessage Clone()
        {
            return new GenericMessage()
            {
                Sender = this.Sender,
                Receiver = this.Receiver,
                Command = this.Command,
                RawContent = this.RawContent,
                Server = this.Server,
                Tag = this.Tag
            };
        }
    }
}
