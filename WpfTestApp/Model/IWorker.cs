using System.Threading;
using WpfTestApp.Tasks;

namespace WpfTestApp.Model
{
    public interface IWorker
    {
        void Execute(Job job, IProgressReceiver progressReceiver, CancellationToken cancellationToken);
    }
}
