using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Data;
//using Microsoft.SqlServer.Management.Smo;
//using Microsoft.SqlServer.Management.Common;

namespace CORE.MVC
{
    public class Connection
    {
        //SCOPE_IDENTITY

        #region Declarações

        //private Server server;
        private DataConnectionTransaction transaction = null;
        private long tranID;
        #endregion
        DataConnection data = null;
        #region Propriedades
        public bool UseTransaction { get { return transaction != null; } }
        internal bool GlobalTransaction { get; set; }
        public long? TransactionID { get { return tranID == 0 ? default(long?) : tranID; } }
        #endregion
        //internal DataMapper dataMapper;
        #region Conexão
        public Connection()
        {
            //server = new Server(new ServerConnection(new SqlConnection(con.ConnectionString)));
        }
        /// <summary>
        /// Inicializão e Connection String
        /// </summary>
        /// <param name="ConnectionString">Informe a string de conexão</param>
        public Connection(DataConnection data)
        {
            this.data = data;
        }


        /// <summary>
        /// Cria um SqlCommand associado a conexão actual
        /// </summary>
        /// <returns></returns>
        public IDbCommand CreateComand(bool UseTransaction = true)
        {
            //SqlCommand cmd = new SqlCommand();
            var cmd = data.CreateCommand();
            cmd.Parameters.Clear();
            //cmd.Connection = con;
            //UseTransaction && this.UseTransaction ? transaction : null;
            return cmd;
        }
        #endregion

        #region Transação
        /// <summary>
        /// Inicializa a transação
        /// </summary>
        public void StartTransaction()
        {   //transaction.Commit
            if (transaction == null) { transaction = data.BeginTransaction(); GlobalTransaction = true; tranID = Generation.ID; }
        }
        internal void StartInternalTransaction()
        {   //transaction.Commit
            if (transaction == null) { transaction = data.BeginTransaction(); GlobalTransaction = false; tranID = Generation.ID; }
        }
        /// <summary>
        /// Grava os dados permanentemente na base de dados
        /// </summary>
        public void Commit()
        {
            if (UseTransaction)
            {
                //dataMapper.UpdateTransationID();
                transaction.Commit();
                transaction.Dispose();
                transaction = null;
                GlobalTransaction = false;
                tranID = 0;
                return;
            }
            throw new Exception("Impossível fazer o commit. A transação não foi iniciada!");
        }
        /// <summary>
        /// Desfaz as alterações feitas na base de dados
        /// </summary>
        public void Rollback()
        {
            if (UseTransaction)
            {
                //dataMapper.ResetTransationID();
                transaction.Rollback();
                transaction.Dispose();
                transaction = null;
                GlobalTransaction = false;
                tranID = 0;
                return;
            }
            throw new Exception("Impossível fazer o rollback. A transação não foi iniciada!");
        }
        #endregion

        #region Comando

        public bool ExistsDatabase(string name)
        {

            try
            {
                return Execute<bool>($"SELECT CONVERT(BIT, 1) FROM sys.databases WHERE name = N'{name.Replace("[", "").Replace("]", "")}'");
            }
            catch
            {
                return false;
            }
        }

        //internal bool ExecuteSMO(string query)
        //{
        //   return server.ConnectionContext.ExecuteNonQuery(query) != 0;
        //}

        /// <summary>
        /// Executa query na base de dados
        /// </summary>
        /// <param name="Query"> Query sql</param>
        /// <param name="Parameters">Parametros da query</param>
        /// <returns></returns>
        public bool Execute(string Query, params DataParameter[] Parameters)
        {
            Parameters = Parameters ?? new DataParameter[0];
            return data.Execute(Query, Parameters) > -1;
        }
        /// <summary>
        /// Executa query na base de dados
        /// </summary>
        /// <param name="Query"> Query sql</param>
        /// <param name="Parameters">Parametros da query</param>
        /// <returns></returns>
        public bool Execute(ICollection<string> Query, params DataParameter[] Parameters)
        {
            {
                int a = -1;
                Parameters = Parameters ?? new DataParameter[0];

                foreach (var item in Query)
                {
                    a = data.Execute(item, Parameters);
                    if (a == -1)
                        break;
                }

                return a > -1;
            }
        }

        /// <summary>
        /// Executa query na base de dados e retorna um valor escalar
        /// </summary>
        /// <param name="Query"> Query sql</param>
        /// <param name="Parameters">Parametros da query</param>
        /// <returns></returns>
        public T Execute<T>(string Query, params DataParameter[] Parameters)
        {
            Parameters = Parameters ?? new DataParameter[0];
            var v = data.Execute<T>(Query, Parameters);
            return v == null ? default(T) : v;
        }
        /// <summary>
        /// Executa query e retorna os dados como DataTable
        /// </summary>
        /// <param name="Query"> Query sql</param>
        /// <param name="Parameters">Parametros da query</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string Query, params DataParameter[] Parameters)
        {
            List<DataTable> list = new List<DataTable>();

            DataTable table = new DataTable();
            return table == null ? new DataTable() : table;
        }

        /// <summary>
        /// Executa query e retorna os dados como SqlDataReader
        /// </summary>
        /// <param name="Query"> Query sql</param>
        /// <param name="Parameters">Parametros da query</param>
        /// <returns></returns>
        public DataReader ExecuteDataReader(string Query, params DataParameter[] Parameters)
        {
            Parameters = Parameters ?? new DataParameter[0];
            return data.ExecuteReader(Query, Parameters);
        }

        #endregion
    }
}
