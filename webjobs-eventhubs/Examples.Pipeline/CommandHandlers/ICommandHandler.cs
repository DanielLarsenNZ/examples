using System.Threading.Tasks;

namespace Examples.Pipeline.Commands
{
    public interface ICommandHandler<T> where T : ICommand
    {
        Task Handle(T command);
    }
}
