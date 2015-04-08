using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using WpfTestApp.Model;
using IWin32Window = System.Windows.Forms.IWin32Window;
using MessageBox = System.Windows.MessageBox;

namespace WpfTestApp.Extensions
{
    public static class WindowExtensions
    {
        public static Task<T> InvokeBackgroundAction<T>(this IUIModel model, Func<T> func)
        {
            return Task<T>.Factory.StartNew(func, model.CancellationSource.Token);
        }

        public static Task InvokeBackgroundAction(this IUIModel model, Action action)
        {
            return Task.Factory.StartNew(action, model.CancellationSource.Token);
        }

        public static void InvokeOnDispatcher(this Window window, Action action)
        {
            var actionWrapper = new Action(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    MessageBox.Show(window, message, "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
            window.Dispatcher.BeginInvoke(actionWrapper);
        }

        public static IWin32Window GetWin32Window(this Window window)
        {
            return new Win32WindowWrapper(window);
        }

        private class Win32WindowWrapper : IWin32Window
        {
            public IntPtr Handle { get; private set; }

            public Win32WindowWrapper(Window window)
            {
                Handle = new WindowInteropHelper(window).Handle;
            }
        }
    }
}
