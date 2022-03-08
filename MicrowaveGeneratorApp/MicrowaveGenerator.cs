using System;
using TCPClient;

namespace MicrowaveGeneratorApp
{
    public class MicrowaveGenerator
    {
        private const string ServerAddress = "127.0.0.1";
        private const int Port = 11000;

        private string _name;
        private Version _version;
        private AsynchronousClient _client;
        

        public MicrowaveGenerator(string name, Version version, AsynchronousClient client)
        {
            _name = name;
            _version = version;
            _client = client;
        }

        public void Start()
        {
            _client.StartClient(ServerAddress, Port);
        }
    }
}