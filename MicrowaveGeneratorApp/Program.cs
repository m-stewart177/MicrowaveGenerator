using System;
using TCPClient;

namespace MicrowaveGeneratorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            AsynchronousClient client = new AsynchronousClient();
            MicrowaveGenerator microwaveGenerator = new MicrowaveGenerator("mw-1", Version.Debug, client);
            microwaveGenerator.Start();
            Console.ReadKey();
        }
    }
}