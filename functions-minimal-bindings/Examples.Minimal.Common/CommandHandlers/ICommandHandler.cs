using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Minimal.Commands
{
    public interface ICommandHandler<T>
    {
        Task Handle(T command);
    }
}
