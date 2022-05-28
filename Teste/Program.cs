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
            //var db = new TESTEDB();
            //db.Find<Produto>().Select(a => new {a.IDCategoria,a.Nome,Categoria=new {a.Categoria.Nome } }).All();
            var list = Search.Find<Produto>().All();
            //CORE.MVC.Search.Find<Produto>().
            //Categoria categoria = new Categoria();
            //categoria.Nome = "ELETRONICOS";
            //categoria.Save();

            //var rs = p.Save(); //db.Save(p);
            //var query = CORE.MVC.DataMapper.GetSQL<Produto>(a=>a.IsStock==false);
            //Console.WriteLine(query);
            Console.ReadKey();
        }
    }
}
