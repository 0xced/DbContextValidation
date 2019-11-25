using System;

namespace Xunit.Fixture.Docker
{
    /// <summary>
    /// The ContainerId provides a strongly-typed identity handle to running docker containers.
    /// </summary>
    public class ContainerId
    {
        private readonly string _value;

        /// <summary>
        /// Initialize a new instance of the <see cref="ContainerId" /> class.
        /// </summary>
        /// <param name="value">The docker container id.</param>
        public ContainerId(string value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Returns the docker container id as a string.
        /// </summary>
        /// <returns>The docker container id as a string.</returns>
        public override string ToString() => _value;
    }
}