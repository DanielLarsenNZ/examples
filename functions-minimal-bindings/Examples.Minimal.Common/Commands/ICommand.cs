using System;
using System.Collections.Generic;
using System.Text;

namespace Examples.Minimal.Commands
{
    public interface ICommand
    {
        Guid Id { get; set; }
        string CommandType { get; }
    }
}
