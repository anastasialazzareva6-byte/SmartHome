using System;

namespace SmartHomeSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Умный Дом - Система управления";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            SmartHomeSystem smartHome = new SmartHomeSystem();
            smartHome.Run();
        }
    }
}