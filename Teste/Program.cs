using System;
using System.Linq;

namespace TESTE
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //var db = new DataMapper();
            //Categoria categoria = new Categoria();
            //categoria.Nome = "ELETRONICOS";
            //categoria.Save();
            
            //var rs = p.Save(); //db.Save(p);
            var query = CORE.MVC.DataMapper.Search<Produto>().ToList();
        }
    }
}
