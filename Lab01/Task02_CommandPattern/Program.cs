using System;
using System.Collections.Generic;
using System.Threading;

namespace CommandPattern
{
    public interface ICommand
    {
        void Execute();
    }

    public class CreateCommand : ICommand
    {
        private readonly int id;

        public CreateCommand(int id)
        {
            this.id = id;
        }

        public void Execute()
        {
            Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] CreateCommand #{id}: Создание нового объекта...");
            Thread.Sleep(Random.Shared.Next(100, 300)); // Имитация работы
            Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] CreateCommand #{id}: Объект создан успешно!");
        }
    }

    public class UpdateCommand : ICommand
    {
        private readonly int id;

        public UpdateCommand(int id)
        {
            this.id = id;
        }

        public void Execute()
        {
            Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] UpdateCommand #{id}: Обновление данных...");
            Thread.Sleep(Random.Shared.Next(100, 300)); // Имитация работы
            Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] UpdateCommand #{id}: Данные обновлены!");
        }
    }

    public class DeleteCommand : ICommand
    {
        private readonly int id;

        public DeleteCommand(int id)
        {
            this.id = id;
        }

        public void Execute()
        {
            Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] DeleteCommand #{id}: Удаление объекта...");
            Thread.Sleep(Random.Shared.Next(100, 300)); // Имитация работы
            Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] DeleteCommand #{id}: Объект удален!");
        }
    }

    public class CommandProcessor
    {
        private readonly Queue<ICommand> commandQueue;
        private readonly object lockObject = new object();
        private int completedCommands = 0;
        private readonly ManualResetEvent allCommandsCompleted;

        public CommandProcessor()
        {
            commandQueue = new Queue<ICommand>();
            allCommandsCompleted = new ManualResetEvent(false);
        }

        public void AddCommand(ICommand command)
        {
            lock (lockObject)
            {
                commandQueue.Enqueue(command);
            }
        }

        public void ProcessAllCommands()
        {
            int totalCommands;

            lock (lockObject)
            {
                totalCommands = commandQueue.Count;
            }

            Console.WriteLine($"\n=== Начало обработки {totalCommands} команд ===\n");

            for (int i = 0; i < totalCommands; i++)
            {
                ThreadPool.QueueUserWorkItem(ProcessCommand);
            }

            allCommandsCompleted.WaitOne();
            Console.WriteLine("\n=== Все команды обработаны ===");
        }

        private void ProcessCommand(object state)
        {
            ICommand command = null;

            lock (lockObject)
            {
                if (commandQueue.Count > 0)
                {
                    command = commandQueue.Dequeue();
                }
            }

            if (command != null)
            {
                try
                {
                    command.Execute();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Ошибка: {ex.Message}");
                }

                int completed = Interlocked.Increment(ref completedCommands);

                lock (lockObject)
                {
                    if (completed == 10)
                    {
                        allCommandsCompleted.Set();
                    }
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ThreadPool с паттерном Command ===");

            CommandProcessor processor = new CommandProcessor();
            Random random = new Random();

            Type[] commandTypes = { typeof(CreateCommand), typeof(UpdateCommand), typeof(DeleteCommand) };

            Console.WriteLine("Генерация команд:");
            for (int i = 1; i <= 10; i++)
            {
                Type commandType = commandTypes[random.Next(commandTypes.Length)];
                ICommand command;

                if (commandType == typeof(CreateCommand))
                {
                    command = new CreateCommand(i);
                    Console.WriteLine($"  Добавлена CreateCommand #{i}");
                }
                else if (commandType == typeof(UpdateCommand))
                {
                    command = new UpdateCommand(i);
                    Console.WriteLine($"  Добавлена UpdateCommand #{i}");
                }
                else
                {
                    command = new DeleteCommand(i);
                    Console.WriteLine($"  Добавлена DeleteCommand #{i}");
                }

                processor.AddCommand(command);
            }

            processor.ProcessAllCommands();

            Console.WriteLine("\nПрограмма завершена. Нажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}