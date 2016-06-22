using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ContextSwitching
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            ExecuteWorkflow();
        }

        #region Mental Model :)

        private void ConsoleTest()
        {
            Console.WriteLine("coooooooś");
        }

        private async Task ConsoleTestAsync()
        {
            Console.WriteLine("coooooooś");
        }
        #endregion

        #region Context Switching
        private async void OnContextSwitchEventHandler(object sender, RoutedEventArgs e)
        {
            await ContextSwitchAsync();
        }

        public async Task ContextSwitchAsync()
        {
            List<Tuple<string, string>> results = new List<Tuple<string, string>>(3);
            Stopwatch timer = new Stopwatch();

            Debug.WriteLine("Started method with context switching");

            timer.Start();
            await Task.Delay(500);
            timer.Stop();
            PrintThreadId();
            results.Add(new Tuple<string, string>("ConfiguteAwait(true)", timer.ElapsedMilliseconds.ToString()));
            timer.Reset();

            timer.Start();
            var task = Task.Run(() => Task.Delay(500).ConfigureAwait(false).GetAwaiter().GetResult());
            await task;
            timer.Stop();
            PrintThreadId();
            results.Add(new Tuple<string, string>("ConfiguteAwait(false)", timer.ElapsedMilliseconds.ToString()));
            timer.Reset();

            timer.Start();
            await Task.Delay(500).ConfigureAwait(false);
            timer.Stop();
            PrintThreadId();
            results.Add(new Tuple<string, string>("ConfiguteAwait(false)", timer.ElapsedMilliseconds.ToString()));
            timer.Reset();

            string message = string.Empty;
            foreach (var result in results)
            {
                message += string.Format("{0}: {1} ms\n", result.Item1, result.Item2);
            }

            MessageBox.Show(message);
        }

        private void PrintThreadId()
        {
            Debug.WriteLine("ThreadID: {0}", Thread.CurrentThread.ManagedThreadId);
        }
        #endregion

        #region Exceptions
        private void OnCollectClick(object sender, RoutedEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void OnAsyncVoidMethodThrowEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                Throw();
            }
            catch (Exception)
            {
                MessageBox.Show("Jedziemy z catcha");
            }
        }

        private async void Throw()
        {
            await Task.Delay(500);
            throw new InvalidOperationException();
        }

        private async void OnNotAwaitedHotTaskEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                ThrowHotTaskAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetType().Name, "catch");
            }
        }

        private async Task ThrowHotTaskAsync()
        {
            throw new SqlNullValueException();
        }

        private async void OnAwaitedTaskEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                await ThrowAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetType().Name, "catch");
            }
        }

        private async void OnAwaitedHotTaskEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                await ThrowHotTaskAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetType().Name, "catch");
            }
        }

        private async Task ThrowAsync()
        {
            await Task.Delay(100).ConfigureAwait(false);
            throw new InvalidOperationException();
        }

        private void OnWaitedTaskEventHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                ThrowAsync().Wait();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetType().Name, "catch");
            }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.GetType().Name, "UnobservedTaskException");
        }
        #endregion

        #region Async Overhead

        #endregion
        private void OnAsyncOverheadRunEventHandler(object sender, RoutedEventArgs e)
        {
            var stopWatch = new Stopwatch();
            const int ITERATIONS = 1000000;

            //JIT compilation
            AsyncTests.AsyncTest();
            AsyncTests.SyncTest();

            stopWatch.Start();
            for (int i = 0; i < ITERATIONS; i++)
            {
                AsyncTests.SyncTest();
            }
            stopWatch.Stop();
            var ellapsedOnSync = stopWatch.Elapsed;

            stopWatch.Restart();
            for (int i = 0; i < ITERATIONS; i++)
            {
                AsyncTests.AsyncTest();
            }
            stopWatch.Stop();
            var ellapsedOnAsync = stopWatch.Elapsed;

            string message = string.Format("Sync: {0} ms\nAsync: {1} ms\nRatio: {2:F1}", ellapsedOnSync.TotalMilliseconds, ellapsedOnAsync.TotalMilliseconds,
                ellapsedOnAsync.TotalSeconds/ellapsedOnSync.TotalSeconds);

            MessageBox.Show(message);

        }

        #region Workflow

        public async void ExecuteWorkflow()
        {
            await WFB1;
            WFB2.IsEnabled = true;
            await WFB2;
            MessageBox.Show("We are done");
        }
        #endregion
    }

    public static class ButtonExtensions
    {
        public static ButtonAwaiter GetAwaiter(this Button b)
        {
            return new ButtonAwaiter(b);
        }
    }

    public class ButtonAwaiter : INotifyCompletion
    {
        private readonly Button _b;
        private Action _resumeAction;

        public ButtonAwaiter(Button b)
        {
            _b = b;
            _b.Click += _b_Click;
        }

        private void _b_Click(object sender, RoutedEventArgs e)
        {
            _b.Click -= _b_Click;
            _resumeAction();
        }

        public bool IsCompleted
        {
            get { return false; }
        }

        public void OnCompleted(Action continuation)
        {
            _resumeAction = continuation;
        }

        public int GetResult()
        {
            return _b.GetHashCode();
        }
    }
}


