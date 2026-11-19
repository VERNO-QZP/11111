using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp20
{
    internal class Program
    {
        // Сайты для проверки подключения (пункт 4)
        static string[] sites = { "google.com", "ya.ru", "8.8.8.8" };

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("\n========== МЕНЮ ==========\n1. Информация о пользователе\n2. Имя компьютера\n3. Информация о сети\n4. Проверка подключения (ping)\n5. Список процессов\n6. Информация о системе\n7. Сетевые подключения\n8. Сохранить диагностику в файл\n0. Выход\nВыберите пункт: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine(Section("Информация о пользователе", RunCommand("whoami", "/all")));
                        break;
                    case "2":
                        Console.WriteLine(Section("Имя компьютера", RunCommand("hostname", "")));
                        break;
                    case "3":
                        Console.WriteLine(Section("Информация о сети", RunCommand("ipconfig", "/all")));
                        break;
                    case "4":
                        Console.WriteLine("Идёт проверка, подождите...");
                        Console.WriteLine(Section("Проверка подключения", PingSites()));
                        break;
                    case "5":
                        Console.WriteLine(Section("Список процессов", RunCommand("tasklist", "")));
                        break;
                    case "6":
                        Console.WriteLine("Сбор информации, подождите...");
                        Console.WriteLine(Section("Информация о системе", RunCommand("systeminfo", "")));
                        break;
                    case "7":
                        Console.WriteLine(Section("Сетевые подключения", RunCommand("netstat", "-an")));
                        break;
                    case "8":
                        SaveDiagnostics();
                        break;
                    case "0":
                        Console.WriteLine("Выход из программы.");
                        return;
                    default:
                        Console.WriteLine("Ошибка: такого пункта нет, введите число от 0 до 8.");
                        break;
                }
            }
        }

        // Запуск системной команды и получение результата (с обработкой ошибок)
        static string RunCommand(string command, string arguments)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = command;
                info.Arguments = arguments;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                info.StandardOutputEncoding = Encoding.GetEncoding(866); // чтобы русский текст не был кракозябрами
                info.StandardErrorEncoding = Encoding.GetEncoding(866);

                using Process process = Process.Start(info)!;
                Task<string> errorTask = process.StandardError.ReadToEndAsync();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                string error = errorTask.Result;

                if (process.ExitCode != 0)
                {
                    result += $"\n[ОШИБКА] Команда \"{command} {arguments}\" завершилась с кодом {process.ExitCode}\n{error}";
                }
                else if (error.Trim() != "")
                {
                    result += "\n[ПРЕДУПРЕЖДЕНИЕ] " + error;
                }

                return result;
            }
            catch (Exception ex)
            {
                return $"[ОШИБКА] Не удалось выполнить команду \"{command}\": {ex.Message}";
            }
        }

        // Ping минимум 3 сайтов
        static string PingSites()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string site in sites)
            {
                sb.AppendLine($"--- {site} ---");
                sb.AppendLine(RunCommand("ping", "-n 3 " + site));
            }
            return sb.ToString();
        }

        // Оформление заголовка раздела
        static string Section(string title, string text)
        {
            return $"\n========== {title} ==========\n{text}";
        }

        // Пункт 8: выполняет все команды и сохраняет в файл
        static void SaveDiagnostics()
        {
            Console.WriteLine("Выполняются все команды, подождите...");

            StringBuilder report = new StringBuilder();
            report.AppendLine("ДИАГНОСТИКА СИСТЕМЫ");
            report.AppendLine("Дата: " + DateTime.Now);
            report.AppendLine(Section("1. Информация о пользователе", RunCommand("whoami", "/all")));
            report.AppendLine(Section("2. Имя компьютера", RunCommand("hostname", "")));
            report.AppendLine(Section("3. Информация о сети", RunCommand("ipconfig", "/all")));
            report.AppendLine(Section("4. Проверка подключения", PingSites()));
            report.AppendLine(Section("5. Список процессов", RunCommand("tasklist", "")));
            report.AppendLine(Section("6. Информация о системе", RunCommand("systeminfo", "")));
            report.AppendLine(Section("7. Сетевые подключения", RunCommand("netstat", "-an")));

            WriteText(@"C:\Users\Академия\Desktop\diagnostika.txt", report.ToString());
        }

        // Запись текста в файл (с обработкой ошибок)
        static void WriteText(string path, string text)
        {
            try
            {
                File.WriteAllText(path, text, Encoding.UTF8);
                Console.WriteLine("Диагностика сохранена в файл: " + path);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ОШИБКА] Не удалось сохранить файл: " + ex.Message);
            }
        }
    }
}
