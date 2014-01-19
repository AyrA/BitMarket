namespace GenericHandler
{
    public interface GenericProcessor
    {
        event GenericServerLogHandler GenericServerLog;

        /// <summary>
        /// Gets if the server is running
        /// </summary>
        bool IsRunning
        { get; }

        GenericMessage Process(GenericMessage MSG);

        void Start();

        void Config(bool GUI);

        void Stop();
    }
}
