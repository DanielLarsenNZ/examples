using System;

namespace Examples.Pipeline.Commands
{
    public interface ICommand
    {
        Guid Id { get; set; }
        string CommandType { get; }
    }
}
