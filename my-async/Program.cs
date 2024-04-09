using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

// ------------------------------------------------------------

// Prints a thousand 1000 because by the time that it runs
// the 'i' value is already equals '1000'.
//for (int i = 0; i < 1000; i++)
//{
//    ThreadPool.QueueUserWorkItem(_ =>
//    {
//        Console.WriteLine(i);
//        Thread.Sleep(1000);
//    });
//}

// ------------------------------------------------------------

// Prints the value in crescent order because the value of 'i'
// is being captured in each iteration scope.
//for (int i = 0; i < 1000; i++)
//{
//    int capturedValue = i;
//    ThreadPool.QueueUserWorkItem(_ =>
//    {
//        Console.WriteLine(capturedValue);
//        Thread.Sleep(1000);
//    });
//}

// ------------------------------------------------------------

//MyTask.Delay(2000).ContinueWith(() =>
//{
//    Console.Write("hey, ");
//    return MyTask.Delay(2000).ContinueWith(() =>
//    {
//        Console.Write("ho, ");
//        return MyTask.Delay(2000).ContinueWith(() =>
//        {
//            Console.Write("let's go!");
//        });
//    });
//}).Wait();

// ------------------------------------------------------------

//MyTask.Iterate(PrintAsync()).Wait();
//static IEnumerable<MyTask> PrintAsync()
//{
//    for (int i = 0; ; i++)
//    {
//        yield return MyTask.Delay(1000);
//        Console.WriteLine(i);
//    }
//}

// ------------------------------------------------------------

PrintAsync().Wait();
static async Task PrintAsync()
{
    for (int i = 0; ; i++)
    {
        await MyTask.Delay(500);
        Console.WriteLine(i);
    }
}

// ------------------------------------------------------------

class MyTask
{
    private object _lock = new();
    private bool _completed;
    private Exception? _exception;
    private Action? _continuation;
    private ExecutionContext? _context;

    public struct Awaiter(MyTask t) : INotifyCompletion
    {
        public Awaiter GetAwaiter() => this;

        public bool IsCompleted => t.IsCompleted;

        public void OnCompleted(Action continuation) => t.ContinueWith(continuation);

        public void GetResult() => t.Wait();
    }

    public Awaiter GetAwaiter() => new(this);

    public bool IsCompleted
    {
        get
        {
            lock (_lock)
            {
                return _completed;
            }
        }
    }

    public void SetResult() => Complete(null);

    public void SetException(Exception exception) => Complete(exception);

    public void Complete(Exception? exception)
    {
        lock (_lock)
        {
            if (_completed) throw new InvalidOperationException("This task was already completed.");

            _completed = true;
            _exception = exception;

            if (_continuation is not null)
            {
                MyThreadPool.QueueUserWorkItem(() =>
                {
                    if (_context is null)
                        _continuation();
                    else
                        ExecutionContext.Run(_context, state => ((Action)state!).Invoke(), _continuation);
                });
            }
        }
    }

    public void Wait()
    {
        // Stephen Toub's: "ManualResetEvent is just a very thin wrapper around the OS kernel's equivalent
        // primitive, and every time I do any operation on it we're paying a fair amount of overhead to dive
        // down into the kernel. ManualResetEventSlim is a much lighter weight version of it, that's  all
        // implemented in user code in .NET world just based on Monitors (what 'lock' is also built on top of).
        // The only time it is less appropriate to use (the slim version) is when it's necessary to use the
        // kernel construct, which you tipically only need when doing something more exoteric with Wait handles."
        ManualResetEventSlim? mres = null;

        lock (_lock)
        {
            if (!_completed)
            {
                // It will wait only if the task is not already completed
                mres = new ManualResetEventSlim();
                ContinueWith(mres.Set); // This allows to unblock as soon as the task completes
            }
        }

        // It will wait only if a manual reset event slim was instatiated
        mres?.Wait();

        if (_exception is not null)
        {
            // throw _exception; -- it will erase the stack information
            // throw new Exception("", _exception); -- oldest way of preserving the stack information in the inner exception
            // throw new AggregateException(_exception); -- old new way of preserving the stack information
            ExceptionDispatchInfo.Throw(_exception); // new way of throwing existing exception while preserving the stack information (since Task was introduced)
        }
    }

    public MyTask ContinueWith(Action action)
    {
        MyTask t = new();

        Action callback = () =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                t.SetException(ex);
                return;
            }

            t.SetResult();
        };

        lock (_lock)
        {
            if (_completed)
            {
                MyThreadPool.QueueUserWorkItem(callback);
            }
            else
            {
                // If it is not completed yet, we need to store the continuation action and capture the
                // current thread local context for later use.
                _continuation = callback;
                _context = ExecutionContext.Capture();
            }
        }

        return t;
    }

    public MyTask ContinueWith(Func<MyTask> action)
    {
        MyTask t = new();

        Action callback = () =>
        {
            try
            {
                MyTask next = action();
                next.ContinueWith(() =>
                {
                    if (next._exception is not null)
                    {
                        t.SetException(next._exception);
                    }
                    else
                    {
                        t.SetResult();
                    }
                });
            }
            catch (Exception ex)
            {
                t.SetException(ex);
                return;
            }
        };

        lock (_lock)
        {
            if (_completed)
            {
                // If it is already completed, just run the continuation
                // No need to capture the context since it will be run in the same thread, synchronously.
                MyThreadPool.QueueUserWorkItem(callback);
            }
            else
            {
                // If it is not completed yet, we need to store the continuation action and capture the
                // current thread local context for later use.
                _continuation = callback;
                _context = ExecutionContext.Capture();
            }
        }

        return t;
    }

    public static MyTask Run(Action action)
    {
        MyTask t = new();

        MyThreadPool.QueueUserWorkItem(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                t.SetException(ex);
                return;
            }

            t.SetResult();
        });

        return t;
    }

    public static MyTask WhenAll(List<MyTask> tasks)
    {
        MyTask t = new();

        if (tasks.Count == 0)
        {
            t.SetResult();
        }
        else
        {
            int remaining = tasks.Count;

            Action continuation = () =>
            {
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    // TODO: exceptions
                    t.SetResult();
                }
            };

            foreach (var task in tasks)
            {
                task.ContinueWith(continuation);
            }
        }

        return t;
    }

    public static MyTask Delay(int timeout)
    {
        MyTask t = new();

        // Naive approach: Thread.Sleep puts the thread to sleep in the specified amount of time.
        // This will keep the thread active but doing nothing, wasting a spot on ThreadPool.
        //MyThreadPool.QueueUserWorkItem(() =>
        //{
        //    Thread.Sleep(timeout);
        //});
        new Timer(_ => t.SetResult()).Change(dueTime: timeout, period: Timeout.Infinite);

        return t;
    }

    public static MyTask Iterate(IEnumerable<MyTask> tasks)
    {
        MyTask t = new();

        IEnumerator<MyTask> e = tasks.GetEnumerator();
        void MoveNext()
        {

            try
            {
                if (e.MoveNext())
                {
                    MyTask next = e.Current;
                    next.ContinueWith(MoveNext);
                    return;
                }
            }
            catch (Exception e)
            {
                t.SetException(e);
                return;
            }

            t.SetResult();
        }

        MoveNext();

        return t;
    }
}

static class MyThreadPool
{

    // BlockingCollection is a thread-safe collection of items jthat blocks if it is empty
    // when someone is trying to read it. This way threads will await until there's something
    // for them to consume.
    private static readonly BlockingCollection<(Action, ExecutionContext?)> s_workItems = new();

    public static void QueueUserWorkItem(Action action) => s_workItems.Add((action, ExecutionContext.Capture()));

    static MyThreadPool()
    {
        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            new Thread(_ =>
            {
                while (true)
                {
                    (Action workItem, ExecutionContext? context) = s_workItems.Take();
                    if (context is null)
                        workItem();
                    else
                        ExecutionContext.Run(context, /* object? */ state => ((Action)state!).Invoke(), workItem);
                }
            })
            { IsBackground = true }.Start();
        }
    }
}
