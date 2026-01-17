using System;

namespace SmartHomeSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Устанавливаем кодировку для поддержки русского языка
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                Console.WriteLine("=== Система Умного Дома ===");
                Console.WriteLine("Запуск системы...\n");

                // Создаем и запускаем систему
                SmartHomeSystem system = new SmartHomeSystem();
                system.Run();

                Console.WriteLine("\nСистема завершила работу.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n══════════════════════════════════════════════════════════════════════════");
                Console.WriteLine("КРИТИЧЕСКАЯ ОШИБКА!");
                Console.WriteLine($"Сообщение: {ex.Message}");
                Console.WriteLine($"Тип ошибки: {ex.GetType().Name}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
                }

                Console.WriteLine("══════════════════════════════════════════════════════════════════════════");
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }
    }
}