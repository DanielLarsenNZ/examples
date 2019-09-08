using System;

namespace Examples.Pipeline.Commands
{
    public interface ICommand
    {
        Guid Id { get; set; }
        string CommandType { get; }

        /// <summary>
        /// Computes a Hash of this command for de-duplication purposes.
        /// </summary>
        string ComputeHash();
    }
}
