using System;

namespace Xunit.Fixture.Docker
{
    internal class CommandEventArgs : EventArgs
    {
        public CommandEventArgs(string command, string arguments)
        {
            Command = command;
            Arguments = arguments;
        }

        public string Command { get; }
        public string Arguments { get; }
    }
}