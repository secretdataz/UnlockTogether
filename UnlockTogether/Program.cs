using System;

namespace UnlockTogether
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "UnlockTogether - Farm Together event items unlocker";
            new Unlocker().Init();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
