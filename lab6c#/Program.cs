using System;

namespace SmartHomeSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SmartHomeSystem system = new SmartHomeSystem();
                system.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }
    }
}