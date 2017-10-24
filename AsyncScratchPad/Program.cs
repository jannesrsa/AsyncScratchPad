using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncScratchPad
{
    internal class Program
    {
        public static HashSet<string> _files = new HashSet<string>();

        internal static void Main(string[] args)
        {
            try
            {
                WorkerMethod(ConsoleWriteLineWriteFileAsync, nameof(ConsoleWriteLineWriteFileAsync));
                WorkerMethod(ConsoleWriteLineWithTask, nameof(ConsoleWriteLineWithTask));
                WorkerMethod(ConsoleWriteLineSyncWithTask, nameof(ConsoleWriteLineSyncWithTask));
                ConsoleWriteLineSync(nameof(ConsoleWriteLineSync));
                Console.WriteLine("Finished");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                foreach (var file in _files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }

                }
            }

            Console.ReadKey();
        }

        private static void WriteFile(int i, [CallerMemberName] string memberName = "")
        {
            var tmpFile = Path.Combine(Path.GetTempPath(), memberName + i);
            _files.Add(tmpFile);

            var stringBuilder = new StringBuilder();
            for (int j = 0; j < 10000; j++)
            {
                stringBuilder.AppendLine($"{DateTime.Now.ToShortTimeString()} {Thread.CurrentThread.ManagedThreadId}");
            }

            File.WriteAllText(tmpFile, stringBuilder.ToString());
        }

        private static async Task WriteFileAsync(int i, [CallerMemberName] string memberName = "")
        {
            var tmpFile = Path.Combine(Path.GetTempPath(), memberName + i);
            _files.Add(tmpFile);

            var stringBuilder = new StringBuilder();
            for (int j = 0; j < 1000; j++)
            {
                stringBuilder.AppendLine($"{DateTime.Now.ToShortTimeString()} {Thread.CurrentThread.ManagedThreadId}");
            }

            await File.WriteAllTextAsync(tmpFile, stringBuilder.ToString());
        }

        private static async Task ConsoleWriteLineSyncWithTask(int i)
        {
            WriteFile(i);
        }

        private static Task ConsoleWriteLineWithTask(int i)
        {
            WriteFile(i);
            return Task.CompletedTask;
        }

        private static async Task ConsoleWriteLineWriteFileAsync(int i)
        {
            await WriteFileAsync(i);
        }

        private static void WorkerMethod(Func<int, Task> taskFunction, string methodName)
        {
            Console.WriteLine(methodName);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var taskList = new List<Task>();
            for (int i = 0; i < 1000; i++)
            {
                taskList.Add(taskFunction(i));
            }

            Task.WhenAll(taskList.ToArray());

            stopWatch.Stop();

            Console.WriteLine($"WorkerMethod: Time Taken: {stopWatch.ElapsedMilliseconds}ms");
        }

        private static void ConsoleWriteLineSync(string methodName)
        {
            Console.WriteLine(methodName);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < 1000; i++)
            {
                WriteFile(i);
            }

            stopWatch.Stop();

            Console.WriteLine($"WorkerMethod: Time Taken: {stopWatch.ElapsedMilliseconds}ms");
        }
    }
}