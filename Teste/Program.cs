using CORE.MVC;
using System;
using System.Linq;

namespace TESTE
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var list = Search.Find<Produto>().All();
            
            Console.ReadKey();
        }
    }
}
