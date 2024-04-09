using System;
using System.Threading;
/*
    The volatile keyword indicates that a field might be modified by multiple threads
    that are executing at the same time. The compiler, the runtime system, and even
    hardware may rearrange reads and writes to memory locations for performance reasons.
    Fields that are declared volatile are not subject to these optimizations.
    Adding the volatile modifier ensures that all threads will observe volatile writes
    performed by any other thread in the order in which they were performed. There is no
    guarantee of a single total ordering of volatile writes as seen from all threads of execution.
*/
namespace Volatile
{
    class Program
    {
        public static void Main()
        {
            // Create the worker thread object. This does not start the thread.
            Worker workerObject = new Worker();
            Thread workerThread = new Thread(workerObject.DoWork);

            // Start the worker thread.
            workerThread.Start();
            Console.WriteLine("Main thread: starting worker thread...");

            // Loop until the worker thread activates.
            while (!workerThread.IsAlive)
                ;

            // Put the main thread to sleep for 500 milliseconds to
            // allow the worker thread to do some work.
            Thread.Sleep(500);

            // Request that the worker thread stop itself.
            workerObject.RequestStop();

            // Use the Thread.Join method to block the current thread
            // until the object's thread terminates.
            workerThread.Join();
            Console.WriteLine("Main thread: worker thread has terminated.");
        }
        // Sample output:
        // Main thread: starting worker thread...
        // Worker thread: terminating gracefully.
        // Main thread: worker thread has terminated.
    }

    public class Worker
    {
        // This method is called when the thread is started.
        public void DoWork()
        {
            var i = 0;
            while (!_shouldStop)
            {
                Thread.Sleep(200);
                Console.WriteLine($"Work {i++}");
            }
            Console.WriteLine("Worker thread: terminating gracefully.");
        }
        public void RequestStop()
        {
            _shouldStop = true;
        }
        // Keyword volatile is used as a hint to the compiler that this data
        // member is accessed by multiple threads.
        private volatile bool _shouldStop;
    }
}
