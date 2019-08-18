using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Pipeline.Commands
{
    public interface ICommandHandler<T> where T : ICommand
    {
        Task Handle(T command);
    }
}
