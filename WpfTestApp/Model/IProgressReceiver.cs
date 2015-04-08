namespace WpfTestApp.Model
{
    public interface IProgressReceiver
    {
        void SendProgress(string progress, params object[] args);
    }
}
