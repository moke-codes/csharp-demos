using System;
using System.Diagnostics;
using System.Text;
using static System.Console;

namespace garbageCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            var numberOfTimes = 500_000_000;

            LogGCStatistics(new SimpleConcat(numberOfTimes));
            LogGCStatistics(new StringBuilderConcat(numberOfTimes));
        }

        private static void LogGCStatistics(BaseCommand command)
        {
            GC.Collect();

            var gen0Count = GC.CollectionCount(0);
            var gen1Count = GC.CollectionCount(1);
            var gen2Count = GC.CollectionCount(2);

            Console.WriteLine($"Executing {command.GetType()} test");

            var sw = new Stopwatch();
            sw.Start();

            command.Execute();

            sw.Stop();

            gen0Count = GC.CollectionCount(0) - gen0Count;
            gen1Count = GC.CollectionCount(1) - gen1Count;
            gen2Count = GC.CollectionCount(2) - gen2Count;

            WriteLine($"Elapsed time: {sw.ElapsedMilliseconds / 1000} seconds");
            WriteLine($"Gen 0 count: {gen0Count}");
            WriteLine($"Gen 1 count: {gen1Count}");
            WriteLine($"Gen 2 count: {gen2Count}");
            
            Console.WriteLine();

            GC.Collect();
        }
    }

    public abstract class BaseCommand
    {
        public abstract void Execute();

        protected void RunNTimes(Action<int> func, int numberOfTimes)
        {
            for (int i = 0; i < numberOfTimes; i++)
                func(i);
        }
    }

    public class StringBuilderConcat : BaseCommand
    {
        int _numberOfTimes;

        public StringBuilderConcat(int numberOfTimes)
        {
            _numberOfTimes = numberOfTimes;
        }

        public override void Execute()
        {
            var element = new StringBuilder();

            this.RunNTimes((i) => {
                element.Clear().Append("teste");

                _ = element.Append(" ").Append(i).ToString();
            }, 
            _numberOfTimes);
        }
    }

    public class SimpleConcat : BaseCommand
    {
        int _numberOfTimes;

        public SimpleConcat(int numberOfTimes)
        {
            _numberOfTimes = numberOfTimes;
        }

        public override void Execute()
        {
            this.RunNTimes((i) =>
            {
                var element = "test";

                _ = element + " " + i.ToString();
            }, 
            _numberOfTimes);
        }
    }
}
