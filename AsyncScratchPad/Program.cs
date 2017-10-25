using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncScratchPad
{
    internal class Program
    {
        public static readonly string _fileContent = InitStringBuilder();
        public static readonly Queue<string> _files = new Queue<string>();

        private const int _fileCount = 10;
        private const int _fileLines = 100000;

        internal static void Main(string[] args)
        {
            try
            {
                ConsoleWriteLine_Sync(nameof(ConsoleWriteLine_Sync));

                WorkerMethod(ConsoleWriteLine_WriteFileAsync, nameof(ConsoleWriteLine_WriteFileAsync)).Wait();
                WorkerMethod(ConsoleWriteLine_WithTaskComplete_End, nameof(ConsoleWriteLine_WithTaskComplete_End)).Wait();
                WorkerMethod(ConsoleWriteLine_SyncWithTask, nameof(ConsoleWriteLine_SyncWithTask)).Wait();
                WorkerMethod(ConsoleWriteLine_WithTaskRun, nameof(ConsoleWriteLine_WithTaskRun)).Wait();
                WorkerMethod(ConsoleWriteLine_WithTaskDelay0, nameof(ConsoleWriteLine_WithTaskDelay0)).Wait();
                WorkerMethod(ConsoleWriteLine_WithTaskCompletedTask_Begin, nameof(ConsoleWriteLine_WithTaskCompletedTask_Begin)).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Finished. Press any key to delete all files");
                Console.ReadKey();

                Console.WriteLine("Delete started ...");
                while (_files.TryDequeue(out string file))
                {
                    try
                    {
                        File.Delete(file);
                        Console.Write(".");
                    }
                    catch { }
                }

                Console.WriteLine();
                Console.WriteLine("Delete end");
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void ConsoleWriteLine_Sync(string methodName)
        {
            Console.WriteLine(methodName);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < _fileCount; i++)
            {
                WriteFile(i, methodName);
            }

            stopWatch.Stop();

            Console.WriteLine($"WorkerMethod: Time Taken: {stopWatch.ElapsedMilliseconds}ms");
        }

        private static async Task ConsoleWriteLine_SyncWithTask(int i, string methodName)
        {
            WriteFile(i, methodName);
            return;
        }

        private static Task ConsoleWriteLine_WithTaskComplete_End(int i, string methodName)
        {
            WriteFile(i, methodName);
            return Task.CompletedTask;
        }

        private static async Task ConsoleWriteLine_WithTaskDelay0(int i, string methodName)
        {
            await Task.Delay(0);
            WriteFile(i, methodName);
        }

        private static async Task ConsoleWriteLine_WithTaskCompletedTask_Begin(int i, string methodName)
        {
            await Task.CompletedTask;
            WriteFile(i, methodName);
        }

        private static Task ConsoleWriteLine_WithTaskRun(int i, string methodName)
        {
            var task = Task.Run(() => WriteFile(i, methodName));
            return task;
        }

        private static async Task ConsoleWriteLine_WriteFileAsync(int i, string methodName)
        {
            await WriteFileAsync(i, methodName);
        }

        private static string InitStringBuilder()
        {
            var returnVal = new StringBuilder();
            for (int j = 0; j < _fileLines; j++)
            {
                returnVal.AppendLine($"{DateTime.Now.ToString()} ");
            }

            return returnVal.ToString();
        }

        private static async Task WorkerMethod(Func<int, string, Task> taskFunction, string methodName)
        {
            Console.WriteLine(methodName);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var taskList = new List<Task>();
            for (int i = 0; i < _fileCount; i++)
            {
                await taskFunction(i, methodName);
            }

            stopWatch.Stop();

            Console.WriteLine($"WorkerMethod: Time Taken: {stopWatch.ElapsedMilliseconds}ms");
        }

        private static void WriteFile(int i, string memberName)
        {
            var tmpFile = GetFileName(i, memberName);
            File.WriteAllText(tmpFile, _fileContent);
        }

        private static async Task WriteFileAsync(int i, string memberName)
        {
            var tmpFile = GetFileName(i, memberName);
            await File.WriteAllTextAsync(tmpFile, _fileContent);
        }

        private static string GetFileName(int i, string memberName)
        {
            var tmpFile = Path.Combine(Path.GetTempPath(), $"{memberName}_{i}-{Thread.CurrentThread.ManagedThreadId}");
            _files.Enqueue(tmpFile);

            return tmpFile;
        }
    }
}