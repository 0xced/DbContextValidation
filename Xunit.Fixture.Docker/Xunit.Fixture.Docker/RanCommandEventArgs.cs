namespace Xunit.Fixture.Docker
{
    internal class RanCommandEventArgs : CommandEventArgs
    {
        public RanCommandEventArgs(string command, string arguments, string output) : base(command, arguments)
        {
            Output = output;
        }

        public string Output { get; }
    }
}