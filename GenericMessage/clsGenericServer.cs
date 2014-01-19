namespace GenericHandler
{
    /// <summary>
    /// Server has a message to process
    /// </summary>
    /// <param name="MSG">Generic Message</param>
    public delegate void GenericMessageReceivedHandler(GenericMessage MSG);
    /// <summary>
    /// Write an entry to log file
    /// </summary>
    /// <param name="sender">Server, that created the message</param>
    /// <param name="GLT">Type of message</param>
    /// <param name="display">Display to the user (alert box or console)</param>
    /// <param name="Text">Text of message</param>
    public delegate void GenericServerLogHandler(object sender, GenericLogType GLT,bool display,string Text);

    /// <summary>
    /// Various types of event messages
    /// </summary>
    public enum GenericLogType
    {
        /// <summary>
        /// Error. User should investigate, but execution can continue
        /// </summary>
        Error,
        /// <summary>
        /// Information message.
        /// </summary>
        Info,
        /// <summary>
        /// Debug Message (developers only)
        /// </summary>
        Debug,
        /// <summary>
        /// Warning message, user should investigate on repeated occurence
        /// </summary>
        Warning,
        /// <summary>
        /// Fatal error. Server will terminate.
        /// </summary>
        Fatal
    }

    /// <summary>
    /// Provides a generic server for message processing
    /// </summary>
    public interface GenericServer
    {
        /// <summary>
        /// Message has arrived
        /// </summary>
        event GenericMessageReceivedHandler GenericMessageReceived;

        /// <summary>
        /// Log message
        /// </summary>
        event GenericServerLogHandler GenericServerLog;

        /// <summary>
        /// Gets if the server is running
        /// </summary>
        bool IsRunning
        { get; }

        /// <summary>
        /// Starts the server component
        /// </summary>
        void Start();

        /// <summary>
        /// Shows server configuration for the user
        /// </summary>
        /// <param name="GUI">true, if GUI, otherwise console, usually BitMarket Clients use true and servers false</param>
        void Config(bool GUI);

        /// <summary>
        /// Stops the Server
        /// </summary>
        void Stop();

        /// <summary>
        /// Deletes the specific Message
        /// </summary>
        /// <param name="MSG">Message</param>
        void DeleteMessage(GenericMessage MSG);

        /// <summary>
        /// </summary>
        /// Sends a message
        /// <param name="MSG">Generic Message to send</param>
        void Send(GenericMessage MSG);

        /// <summary>
        /// Broadcasts updates (if the server supports it)
        /// </summary>
        /// <param name="MSG">Message with broadcast information (Command any RawContent at least)</param>
        void Broadcast(GenericMessage MSG);
    }
}
