namespace CORE.MVC
{
    internal interface IConfiguration
    {
        /// <summary>
        /// Configurar string de conexão
        /// </summary>
        /// <param name="ConectionString">String de conexão</param>
        void Connection(string ConectionString = @"Data Source=.\sqlexpress;Initial Catalog=master;Integrated Security=True");
        /// <summary>
        /// Carregamento inicial
        /// </summary>
        void Standard();
    }
}
