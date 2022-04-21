using System;
using System.Linq;
using CORE.MVC;
using LinqToDB;
using LinqToDB.Common;

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
            Produto p = new Produto()
            {
                Nome = "SSD - 580 GB",
                Categoria = new Categoria
                {
                    Nome = "HD"
                }
            };
            var rs = p.Save(); //db.Save(p);
            var query = CORE.MVC.DataMapper.Search<Produto>().ToList();
            
        }
    }
}
