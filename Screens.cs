using System;
using System.Collections.Generic;
using System.Text;
using TimeMachine.Core;

namespace App
{
    internal static class Screens
    {
        public static string DataFilePath => Path.Combine(Environment.CurrentDirectory, "data.txt");
        private static CancellationTokenSource _cts = null;
        public static void WriteData()
        {
            start:
            Console.Clear();
            Console.WriteLine("Введите дату и время в формате: yyyy-MM-dd HH:mm:ss");
            Console.WriteLine("Для выхода нажмите Enter");
            var input = Console.ReadLine();
            if (string.IsNullOrEmpty(input)) return;
            Console.WriteLine("Сохранение...");
            if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var dateTime))
            {
                SharedTimeProvider.Write(dateTime);
                Console.WriteLine("Новая дата и время успешно записаны");
                Console.WriteLine("Для продолжения нажмите любую клавишу...");
                Console.ReadKey(true);
                return;
            }
            else
            {
                Console.WriteLine("Введены некоректные данные");
                Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                Console.ReadKey(true);
                goto start;
            }
        }

        public static void LoadData()
        {
            Console.Clear();
            Console.WriteLine("Загрузка данных");
            var currentDateTime = SharedTimeProvider.Read();
            if (!File.Exists(DataFilePath))
            {
                Console.WriteLine("Файл с данными отсутвует...");
                Console.WriteLine("Создать новый? Enter - да, любая клавиша - нет");
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter)
                {
                    File.WriteAllText(DataFilePath, currentDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    Console.WriteLine("Для продолжения нажмите любую клавишу...");
                    Console.ReadKey(true);
                    return;
                }
                else
                {
                    Console.WriteLine("Для продолжения нажмите любую клавишу...");
                    Console.ReadKey(true);
                    return;
                }
            }
            var data = File.ReadAllText(DataFilePath);
            Console.WriteLine("Сохранённая дата и время: " + data);
            if (!DateTime.TryParseExact(data, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var savedDateTime))
            {
                Console.WriteLine("Данные в файле не корректны...");
                Console.WriteLine("Перезаписать файл? Enter - да, любая клавиша - нет");
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter)
                {
                    File.WriteAllText(DataFilePath,currentDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    Console.WriteLine("Для продолжения нажмите любую клавишу...");
                    Console.ReadKey(true);
                    return;
                }
                else
                {
                    Console.WriteLine("Для продолжения нажмите любую клавишу...");
                    Console.ReadKey(true);
                    return;
                }
            }
            if (currentDateTime == DateTime.MinValue)
            {
                SharedTimeProvider.Write(savedDateTime);
                Console.WriteLine("Дата и время успешно загружены");
                Console.WriteLine("Для продолжения нажмите любую клавишу...");
                Console.ReadKey(true);
                return;
            }
            
            if (savedDateTime != currentDateTime)
            {
                Console.WriteLine("Текущая дата и время: " + SharedTimeProvider.Read().ToString("yyyy-MM-dd HH:mm:ss"));
                Console.WriteLine("Перезаписать? Enter - да, любая клавиша - нет");
                var key = Console.ReadKey(true).Key;
                if(key == ConsoleKey.Enter)
                {
                    SharedTimeProvider.Write(savedDateTime);
                    Console.WriteLine("Дата и время успешно обновлены");
                    Console.WriteLine("Для продолжения нажмите любую клавишу...");
                    Console.ReadKey(true);
                    return;
                }
                else
                {
                    Console.WriteLine("Для продолжения нажмите любую клавишу...");
                    Console.ReadKey(true);
                    return;
                }
            }
        }

        public static void SaveData()
        {
            Console.Clear();
            var currentDateTime = SharedTimeProvider.Read();
            if(currentDateTime == DateTime.MinValue)
            {
                Console.WriteLine("Данные для сохранения отсутвуют...");
                Console.WriteLine("Для продолжения нажмите любую клавишу...");
                Console.ReadKey(true);
                return;
            }
            Console.WriteLine("Перезаписать файл? Enter - да, любая клавиша - нет");
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Enter)
            {
                File.WriteAllText(DataFilePath, currentDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                Console.WriteLine("Файл успешно перезаписан");
                Console.WriteLine("Для продолжения нажмите любую клавишу...");
                Console.ReadKey(true);
                return;
            }
            else
            {
                Console.WriteLine("Для продолжения нажмите любую клавишу...");
                Console.ReadKey(true);
                return;
            }
        }

        public static async Task StartServiceAsync()
        {
            start:
            Console.Clear();
            var currentDateTime = SharedTimeProvider.Read();
            if(currentDateTime == DateTime.MinValue)
            {
                Console.WriteLine("Данные не загружены, запуск сервиса невозможен");
                Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                Console.ReadKey(true);
                return;
            }
            Console.WriteLine("Запуск сервиса...");
            Console.WriteLine("Введите частоту обновления в секундах:");
            var input = Console.ReadLine();
            if (int.TryParse(input, out var value))
            {
                _cts = new CancellationTokenSource();
                var token = _cts.Token;

                Console.WriteLine("Сервис запущен. Нажмите ESC для остановки...");
                Console.WriteLine();

                var keyPressTask = Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true).Key;
                            if (key == ConsoleKey.Escape)
                            {
                                _cts.Cancel();
                                break;
                            }
                        }
                        Task.Delay(100).Wait();
                    }
                });

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        currentDateTime = currentDateTime.AddSeconds(1);
                        SharedTimeProvider.Write(currentDateTime);
                        Console.WriteLine("Текущая дата: " + currentDateTime.ToString("yyyy-MM-dd"));
                        Console.WriteLine("Текущее время: " + currentDateTime.ToString("HH:mm:ss"));
                        Console.WriteLine("Нажмите ESC для остановки");

                        await Task.Delay(value * 1000, token);
                        Console.Clear();
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.Clear();
                    Console.WriteLine("Текущая дата: " + currentDateTime.ToString("yyyy-MM-dd"));
                    Console.WriteLine("Текущее время: " + currentDateTime.ToString("HH:mm:ss"));
                    Console.WriteLine("Сервис остановлен по запросу пользователя");
                }
                finally
                {
                    _cts?.Dispose();
                    _cts = null;
                }

                Console.WriteLine("Нажмите любую клавишу для продолжения...");
                Console.ReadKey(true);
            }
            else
            {
                Console.WriteLine("Введено некоректное значение");
                Console.WriteLine("Нажмите любую клавишу чтобы продолжить...");
                Console.ReadKey(true);
                goto start;
            }
        }
    }
}
