using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace MultithreadingLab
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Лабораторная работа: Многопоточное программирование ===\n");

            CreateNumbersJsonIfNotExists();

            while (true)
            {
                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("МЕНЮ");
                Console.WriteLine(new string('=', 60));
                Console.WriteLine("1. Задача 1: Обработка массива (однопоточно)");
                Console.WriteLine("2. Задача 1: Обработка массива (многопоточно)");
                Console.WriteLine("3. Задача 2: Суммирование (однопоточно)");
                Console.WriteLine("4. Задача 2: Суммирование (многопоточно с ThreadPool)");
                Console.WriteLine("5. Задача 3: Поиск максимума (однопоточно)");
                Console.WriteLine("6. Задача 3: Поиск максимума (многопоточно)");
                Console.WriteLine("7. Сравнить все задачи (однопоточно vs многопоточно)");
                Console.WriteLine("0. Выход");
                Console.Write("\nВыберите опцию: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        Task1_ArrayProcessing_SingleThread();
                        break;
                    case "2":
                        Task1_ArrayProcessing_MultiThread();
                        break;
                    case "3":
                        Task2_Sum_SingleThread();
                        break;
                    case "4":
                        Task2_Sum_ThreadPool();
                        break;
                    case "5":
                        Task3_FindMax_SingleThread();
                        break;
                    case "6":
                        Task3_FindMax_MultiThread();
                        break;
                    case "7":
                        CompareAll();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void CreateNumbersJsonIfNotExists()
        {
            string filePath = "numbers.json";

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Создание файла numbers.json...");

                Random random = new Random(66);
                int[] largeArray = new int[100000000];
                for (int i = 0; i < largeArray.Length; i++)
                {
                    largeArray[i] = random.Next(1, 1001);
                }

                string json = JsonSerializer.Serialize(largeArray);
                File.WriteAllText(filePath, json);

                Console.WriteLine($"Файл создан с {largeArray.Length} элементами\n");
            }
        }

        static int[] LoadNumbersFromJson()
        {
            try
            {
                string json = File.ReadAllText("numbers.json");
                return JsonSerializer.Deserialize<int[]>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения файла: {ex.Message}");
                return new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            }
        }

        // ЗАДАЧА 1: ОБРАБОТКА МАССИВА
        static void Task1_ArrayProcessing_SingleThread()
        {
            Console.WriteLine("ЗАДАЧА 1: Обработка массива (ОДНОПОТОЧНЫЙ РЕЖИМ)");
            Console.WriteLine(new string('=', 60));

            int[] numbers = LoadNumbersFromJson();
            int[] results = new int[numbers.Length];

            Console.WriteLine($"Загружено элементов: {numbers.Length}");
            Console.WriteLine($"Первые 10 элементов: [{string.Join(", ", numbers.Take(10))}]...\n");

            Console.WriteLine("Обработка массива (умножение каждого элемента на 2)...");

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < numbers.Length; i++)
            {
                results[i] = numbers[i] * 2;
            }

            stopwatch.Stop();

            Console.WriteLine($"\nПервые 10 результатов: [{string.Join(", ", results.Take(10))}]...");
            Console.WriteLine($"Всего обработано элементов: {results.Length}");
            Console.WriteLine($"⏱ Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
        }

        static void Task1_ArrayProcessing_MultiThread()
        {
            Console.WriteLine("ЗАДАЧА 1: Обработка массива (МНОГОПОТОЧНЫЙ РЕЖИМ)");
            Console.WriteLine(new string('=', 60));

            int[] numbers = LoadNumbersFromJson();
            int[] results = new int[numbers.Length];
            int threadCount = Environment.ProcessorCount;

            Console.WriteLine($"Загружено элементов: {numbers.Length}");
            Console.WriteLine($"Первые 10 элементов: [{string.Join(", ", numbers.Take(10))}]...");
            Console.WriteLine($"Количество потоков: {threadCount}\n");

            Stopwatch stopwatch = Stopwatch.StartNew();

            int elementsPerThread = (int)Math.Ceiling((double)numbers.Length / threadCount);
            Thread[] threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                int startIndex = threadIndex * elementsPerThread;
                int endIndex = Math.Min(startIndex + elementsPerThread, numbers.Length);

                threads[i] = new Thread(() =>
                {
                    Console.WriteLine($"  Поток {threadIndex + 1}: обработка элементов [{startIndex}..{endIndex - 1}]");

                    for (int j = startIndex; j < endIndex; j++)
                    {
                        results[j] = numbers[j] * 2;
                    }

                    Console.WriteLine($"  Поток {threadIndex + 1}: завершен");
                });

                threads[i].Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            stopwatch.Stop();

            Console.WriteLine($"\nПервые 10 результатов: [{string.Join(", ", results.Take(10))}]...");
            Console.WriteLine($"Всего обработано элементов: {results.Length}");
            Console.WriteLine($"⏱ Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
        }

        // ЗАДАЧА 2: СУММИРОВАНИЕ
        static void Task2_Sum_SingleThread()
        {
            Console.WriteLine("ЗАДАЧА 2: Суммирование (ОДНОПОТОЧНЫЙ РЕЖИМ)");
            Console.WriteLine(new string('=', 60));

            int[] numbers = LoadNumbersFromJson();
            long totalSum = 0;

            Console.WriteLine($"Загружено элементов: {numbers.Length}");
            Console.WriteLine($"Первые 10 элементов: [{string.Join(", ", numbers.Take(10))}]...\n");

            Console.WriteLine("Вычисление суммы всех элементов...");

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < numbers.Length; i++)
            {
                totalSum += numbers[i];
            }

            stopwatch.Stop();

            Console.WriteLine($"\n✓ Общая сумма: {totalSum:N0}");
            Console.WriteLine($"⏱ Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
        }

        static void Task2_Sum_ThreadPool()
        {
            Console.WriteLine("ЗАДАЧА 2: Суммирование (МНОГОПОТОЧНЫЙ РЕЖИМ - ThreadPool)");
            Console.WriteLine(new string('=', 60));

            object lockObj = new object();
            long totalSum = 0;
            int[] numbers = LoadNumbersFromJson();
            int parts = Environment.ProcessorCount;

            Console.WriteLine($"Загружено элементов: {numbers.Length}");
            Console.WriteLine($"Первые 10 элементов: [{string.Join(", ", numbers.Take(10))}]...");
            Console.WriteLine($"Количество частей: {parts}\n");

            Stopwatch stopwatch = Stopwatch.StartNew();

            int elementsPerPart = (int)Math.Ceiling((double)numbers.Length / parts);
            ManualResetEvent[] doneEvents = new ManualResetEvent[parts];

            for (int i = 0; i < parts; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                int partIndex = i;
                int startIndex = partIndex * elementsPerPart;
                int endIndex = Math.Min(startIndex + elementsPerPart, numbers.Length);

                ThreadPool.QueueUserWorkItem(state =>
                {
                    long partSum = 0;

                    Console.WriteLine($"  Часть {partIndex + 1}: суммирование элементов [{startIndex}..{endIndex - 1}]");

                    for (int j = startIndex; j < endIndex; j++)
                    {
                        partSum += numbers[j];
                    }

                    lock (lockObj)
                    {
                        totalSum += partSum;
                        Console.WriteLine($"  Часть {partIndex + 1}: частичная сумма = {partSum:N0}");
                    }

                    doneEvents[partIndex].Set();
                });
            }

            WaitHandle.WaitAll(doneEvents);

            stopwatch.Stop();

            Console.WriteLine($"\n✓ Общая сумма: {totalSum:N0}");
            Console.WriteLine($"⏱ Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
        }

        // ЗАДАЧА 3: ПОИСК МАКСИМУМА
        static void Task3_FindMax_SingleThread()
        {
            Console.WriteLine("ЗАДАЧА 3: Поиск максимального значения (ОДНОПОТОЧНЫЙ РЕЖИМ)");
            Console.WriteLine(new string('=', 60));

            int[] numbers = LoadNumbersFromJson();
            int maxValue = int.MinValue;

            Console.WriteLine($"Загружено элементов: {numbers.Length}");
            Console.WriteLine($"Первые 10 элементов: [{string.Join(", ", numbers.Take(10))}]...\n");

            Console.WriteLine("Поиск максимального значения...");

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < numbers.Length; i++)
            {
                if (numbers[i] > maxValue)
                {
                    maxValue = numbers[i];
                }
            }

            stopwatch.Stop();

            Console.WriteLine($"\n✓ Максимальное значение: {maxValue}");
            Console.WriteLine($"⏱ Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
        }

        static void Task3_FindMax_MultiThread()
        {
            Console.WriteLine("ЗАДАЧА 3: Поиск максимального значения (МНОГОПОТОЧНЫЙ РЕЖИМ)");
            Console.WriteLine(new string('=', 60));

            int maxValue = int.MinValue;
            object lockObj = new object();
            int[] numbers = LoadNumbersFromJson();
            int threadCount = Environment.ProcessorCount;

            Console.WriteLine($"Загружено элементов: {numbers.Length}");
            Console.WriteLine($"Первые 10 элементов: [{string.Join(", ", numbers.Take(10))}]...");
            Console.WriteLine($"Количество потоков: {threadCount}\n");

            Stopwatch stopwatch = Stopwatch.StartNew();

            int elementsPerThread = (int)Math.Ceiling((double)numbers.Length / threadCount);
            Thread[] threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                int startIndex = threadIndex * elementsPerThread;
                int endIndex = Math.Min(startIndex + elementsPerThread, numbers.Length);

                threads[i] = new Thread(() =>
                {
                    int localMax = int.MinValue;

                    Console.WriteLine($"  Поток {threadIndex + 1}: поиск в элементах [{startIndex}..{endIndex - 1}]");

                    for (int j = startIndex; j < endIndex; j++)
                    {
                        if (numbers[j] > localMax)
                        {
                            localMax = numbers[j];
                        }
                    }

                    lock (lockObj)
                    {
                        Console.WriteLine($"  Поток {threadIndex + 1}: локальный максимум = {localMax}");
                        if (localMax > maxValue)
                        {
                            maxValue = localMax;
                        }
                    }
                });

                threads[i].Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            stopwatch.Stop();

            Console.WriteLine($"\n✓ Максимальное значение: {maxValue}");
            Console.WriteLine($"⏱ Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
        }

        // СРАВНЕНИЕ ВСЕХ ЗАДАЧ
        static void CompareAll()
        {
            Console.WriteLine("СРАВНЕНИЕ: Однопоточный vs Многопоточный режимы");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine();

            int[] numbers = LoadNumbersFromJson();
            Console.WriteLine($"Размер массива: {numbers.Length:N0} элементов");
            Console.WriteLine($"Количество ядер процессора: {Environment.ProcessorCount}");
            Console.WriteLine();

            // Задача 1
            Console.WriteLine("--- ЗАДАЧА 1: Обработка массива ---");

            Stopwatch sw = Stopwatch.StartNew();
            int[] results1 = new int[numbers.Length];
            for (int i = 0; i < numbers.Length; i++)
                results1[i] = numbers[i] * 2;
            long time1Single = sw.ElapsedMilliseconds;
            Console.WriteLine($"  Однопоточно: {time1Single} мс");

            sw.Restart();
            int[] results2 = new int[numbers.Length];
            int threadCount = Environment.ProcessorCount;
            int elementsPerThread = (int)Math.Ceiling((double)numbers.Length / threadCount);
            Thread[] threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                int startIndex = threadIndex * elementsPerThread;
                int endIndex = Math.Min(startIndex + elementsPerThread, numbers.Length);
                threads[i] = new Thread(() =>
                {
                    for (int j = startIndex; j < endIndex; j++)
                        results2[j] = numbers[j] * 2;
                });
                threads[i].Start();
            }
            foreach (var thread in threads)
                thread.Join();
            long time1Multi = sw.ElapsedMilliseconds;
            Console.WriteLine($"  Многопоточно: {time1Multi} мс");
            Console.WriteLine($"  Ускорение: {(double)time1Single / time1Multi:F2}x\n");

            // Задача 2
            Console.WriteLine("--- ЗАДАЧА 2: Суммирование ---");

            sw.Restart();
            long sum1 = 0;
            for (int i = 0; i < numbers.Length; i++)
                sum1 += numbers[i];
            long time2Single = sw.ElapsedMilliseconds;
            Console.WriteLine($"  Однопоточно: {time2Single} мс");

            sw.Restart();
            long sum2 = 0;
            object lockObj = new object();
            int parts = Environment.ProcessorCount;
            int elementsPerPart = (int)Math.Ceiling((double)numbers.Length / parts);
            ManualResetEvent[] doneEvents = new ManualResetEvent[parts];
            for (int i = 0; i < parts; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                int partIndex = i;
                int startIndex = partIndex * elementsPerPart;
                int endIndex = Math.Min(startIndex + elementsPerPart, numbers.Length);
                ThreadPool.QueueUserWorkItem(state =>
                {
                    long partSum = 0;
                    for (int j = startIndex; j < endIndex; j++)
                        partSum += numbers[j];
                    lock (lockObj)
                        sum2 += partSum;
                    doneEvents[partIndex].Set();
                });
            }
            WaitHandle.WaitAll(doneEvents);
            long time2Multi = sw.ElapsedMilliseconds;
            Console.WriteLine($"  Многопоточно: {time2Multi} мс");
            Console.WriteLine($"  Ускорение: {(double)time2Single / time2Multi:F2}x\n");

            // Задача 3
            Console.WriteLine("--- ЗАДАЧА 3: Поиск максимума ---");

            sw.Restart();
            int max1 = int.MinValue;
            for (int i = 0; i < numbers.Length; i++)
                if (numbers[i] > max1)
                    max1 = numbers[i];
            long time3Single = sw.ElapsedMilliseconds;
            Console.WriteLine($"  Однопоточно: {time3Single} мс");

            sw.Restart();
            int max2 = int.MinValue;
            object lockObj2 = new object();
            threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                int startIndex = threadIndex * elementsPerThread;
                int endIndex = Math.Min(startIndex + elementsPerThread, numbers.Length);
                threads[i] = new Thread(() =>
                {
                    int localMax = int.MinValue;
                    for (int j = startIndex; j < endIndex; j++)
                        if (numbers[j] > localMax)
                            localMax = numbers[j];
                    lock (lockObj2)
                        if (localMax > max2)
                            max2 = localMax;
                });
                threads[i].Start();
            }
            foreach (var thread in threads)
                thread.Join();
            long time3Multi = sw.ElapsedMilliseconds;
            Console.WriteLine($"  Многопоточно: {time3Multi} мс");
            Console.WriteLine($"  Ускорение: {(double)time3Single / time3Multi:F2}x\n");

            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Среднее ускорение: {((double)time1Single / time1Multi + (double)time2Single / time2Multi + (double)time3Single / time3Multi) / 3:F2}x");
        }
    }
}