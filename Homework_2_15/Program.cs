using System.Diagnostics;

namespace Homework_2_15
{
    internal class Program
    {
        // 1. Обычное (последовательное) вычисление
        static long SumSequential(int[] arr)
        {
            long sum = 0;
            for (int i = 0; i < arr.Length; i++)
                sum += arr[i];
            return sum;
        }

        // 2. Параллельное через Thread (ручное разбиение на части)
        static long SumThreads(int[] arr)
        {
            int threadCount = Environment.ProcessorCount;
            int chunkSize = arr.Length / threadCount;
            long[] partialSums = new long[threadCount];
            var threads = new List<Thread>(threadCount);

            for (int t = 0; t < threadCount; t++)
            {
                int start = t * chunkSize;
                // последний поток забирает остаток
                int end = (t == threadCount - 1) ? arr.Length : start + chunkSize;
                int idx = t;

                var thread = new Thread(() =>
                {
                    long local = 0;
                    for (int i = start; i < end; i++)
                        local += arr[i];
                    partialSums[idx] = local;
                });

                threads.Add(thread);
                thread.Start();
            }

            foreach (var th in threads)
                th.Join();

            long total = 0;
            foreach (var s in partialSums)
                total += s;
            return total;
        }

        // 3. Параллельное через PLINQ
        static long SumLinq(int[] arr)
        {
            return arr.AsParallel().Sum(x => (long)x);
        }

        // Замер времени
        static long Measure(Func<int[], long> func, int[] arr, out long ms)
        {
            var sw = Stopwatch.StartNew();
            long result = func(arr);
            sw.Stop();
            ms = sw.ElapsedMilliseconds;
            return result;
        }

        static int[] GenerateArray(int size)
        {
            var rnd = new Random(42); // фиксированный seed для воспроизводимости
            var arr = new int[size];
            for (int i = 0; i < size; i++)
                arr[i] = rnd.Next(1, 100);
            return arr;
        }

        static void Main()
        {
            int[] sizes = { 100_000, 1_000_000, 10_000_000 };

            Console.WriteLine($"Процессор: {Environment.ProcessorCount} логических ядер");
            Console.WriteLine($"ОС: {Environment.OSVersion}");
            Console.WriteLine();

            Console.WriteLine($"{"Размер",-12}|{"Послед.",-12}|{"Thread",-12}|{"PLINQ",-12}|{"Sum (проверка)",-15}");
            Console.WriteLine(new string('-', 70));

            foreach (int size in sizes)
            {
                int[] arr = GenerateArray(size);

                // Прогрев (JIT) — важно для честных замеров!
                SumSequential(arr);

                Measure(SumSequential, arr, out long tSeq);
                Measure(SumThreads, arr, out long tThr);
                Measure(SumLinq, arr, out long tLinq);

                long check = SumSequential(arr);

                Console.WriteLine($"{size,-12}|{tSeq + " мс",-12}|{tThr + " мс",-12}|{tLinq + " мс",-12}|{check,-15}");
            }
        }
    }
}
