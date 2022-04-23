namespace TESTE
{
    public class TESTEDB : CORE.MVC.DataMapper
    {
        protected override void Initialize(string ConnectionName = "TESTE")
        {
            this.InsertModel(new Produto()
            {
                Nome = "SSD - 420 GB",
                Categoria = new Categoria
                {
                    Nome = "HD"
                }
            });
        }
    }
}
