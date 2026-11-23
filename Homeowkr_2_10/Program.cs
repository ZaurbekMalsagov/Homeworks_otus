using System.Diagnostics;

namespace Homeowkr_2_10
{
    internal class Program
    {
        static int CountSpaces(string path)
        {
            string text = File.ReadAllText(path);
            int count = 0;
            foreach (char c in text)
                if (c == ' ') count++;
            return count;
        }

        static async Task RunThreeFilesAsync(string folder)
        {
            string[] files =
            {
                Path.Combine(folder, "file1.txt"),
                Path.Combine(folder, "file2.txt"),
                Path.Combine(folder, "file3.txt")
            };

            Task<int>[] tasks = files.Select(f => Task.Run(() => CountSpaces(f))).ToArray();
            int[] results = await Task.WhenAll(tasks);

            for (int i = 0; i < files.Length; i++)
                Console.WriteLine($"{Path.GetFileName(files[i])}: {results[i]} пробелов");
        }

        static async Task RunFolderAsync(string folder)
        {
            string[] files = Directory.GetFiles(folder);

            var tasks = files.Select(f => Task.Run(() => (file: f, spaces: CountSpaces(f)))).ToArray();
            var results = await Task.WhenAll(tasks);

            int total = 0;
            foreach (var (file, spaces) in results)
            {
                Console.WriteLine($"{Path.GetFileName(file)}: {spaces}");
                total += spaces;
            }
            Console.WriteLine($"Итого: {total}");
        }

        static void CreateTestFiles(string folder, int count)
        {
            Directory.CreateDirectory(folder);
            for (int i = 1; i <= count; i++)
            {
                string content = string.Join(" ", Enumerable.Repeat("hello world", 50000 + i * 1000));
                File.WriteAllText(Path.Combine(folder, $"file{i}.txt"), content);
            }
        }

        static async Task Main()
        {
            string folder = Path.Combine(Environment.CurrentDirectory, "testdata");
            CreateTestFiles(folder, 3);

            var sw = Stopwatch.StartNew();
            Console.WriteLine("=== 3 файла параллельно ===");
            await RunThreeFilesAsync(folder);
            sw.Stop();
            Console.WriteLine($"Время (3 файла): {sw.ElapsedMilliseconds} мс");

            sw.Restart();
            Console.WriteLine("\n=== Все файлы папки параллельно ===");
            await RunFolderAsync(folder);
            sw.Stop();
            Console.WriteLine($"Время (папка): {sw.ElapsedMilliseconds} мс");
        }
    }
}
