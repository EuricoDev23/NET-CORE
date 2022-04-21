
using CORE.MVC;
using CORE.MVC.Models;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CORE.MVC
{
    /// <summary>
    /// Manipulador de eventos da base de dados
    /// </summary>
    public class DataMapper
    {
        DataConnection Data_ = null;
        JsonConnetion Config = null;
        string ConnectionName = null;

        public ITable<T> Table<T>() where T:class
        {
            //var tb=DatabaseModel.Instance.Tables.GetTable(type);
            return Data.GetTable<T>();
        }
        public static ITable<T> Search<T>() where T : class
        {
            //var tb=DatabaseModel.Instance.Tables.GetTable(type);
            return new DataMapper().Table<T>();
        }
        public bool ExistsDatabase(string name)
        {

            try
            {
                return Data.Execute<bool>($"SELECT CONVERT(BIT, 1) FROM sys.databases WHERE name = N'{name.Replace("[", "").Replace("]", "")}'");
            }
            catch
            {
                return false;
            }
        }

        #region Declarações
        public enum StateMode {
            None = 0,
            Running = 2,
            Ready = 3,
            Error = 1
        }
        private Connection db = null;
        #endregion

        #region Propriedades
        public Connection Connection { get {

                if (db == null)
                {
                    db = new Connection(Data_);
                }
                return db;
            } }
        public DataConnection Data => Data_;
        /// <summary>
        /// Estado do Mapeador de objecto
        /// </summary>
        public StateMode State => DatabaseModel.State;
        //internal static DataMapper Instance = new DataMapper();
        #endregion
        /// <summary>
        /// Inicializar o framework  
        /// </summary>
        /// <param name="DataBaseName">Nome predefinido da base de dados </param>
        /// <returns></returns>
        public static Result Start()
        {
            var rs = new Result();
            try
            {
               var dt= Activator.CreateInstance<DataMapper>();
                //dt.Init();
            }
            catch (Exception ex)
            {
                rs.AddError(ex);
            }
            return rs;
        }
        public static void DropDatabases()
        {
            CORE.MVC.Generator.Commands.Database.DropDataBases(new DataMapper());
        }
        public DataMapper()
        {
            Init();
        }
        
        #region Init
        internal DataMapper dataMapperInternal;

        /// <summary>
        /// Inicializar o framework e Carregar eventos iniciais
        /// </summary>
        /// <param name="ConnectionString">Conexão</param>
        /// <param name="DataBaseName">Nome padrão</param>
        protected virtual void Initialize(string ConnectionName = "Teste")
        {
            this.ConnectionName = ConnectionName;
            if (DatabaseConsts.JsonConfig == null)
            {
                DatabaseConsts.JsonConfig = Reflection.Database.GetStringByDatabases();
            }
            Config = DatabaseConsts.JsonConfig.Connetions[ConnectionName];
            DatabaseConsts.DefaultBD = string.IsNullOrWhiteSpace(Config.DatabaseName) ? AppDomain.CurrentDomain.FriendlyName.Replace(" ", "_").Split('.')[0] : Config.DatabaseName;
        }
        /// <summary>
        /// Inserir dados padrão
        /// </summary>
        /// <param name="entity"></param>
        protected void InsertModel(Entity entity)
        {
            string db = DatabaseModel.Instance.Tables.GetTable(entity.GetType()).Name.Split('.')[0];
            if (/*db == DatabaseConsts.ActiveBD &&*/  State == StateMode.Running)
            {
                dataMapperInternal.Save(entity).Validate();
            }
        }
        /// <summary>
        /// Inserir dados padrão
        /// </summary>
        /// <param name="models"></param>
        protected void InsertModel(IEnumerable<Entity> models)
        {
            foreach (Entity item in models)
            {
                if(item !=null){
                    InsertModel(item);
                }
            }
        }
        
        private void Init()
        {
            Initialize();
            try
            {
                Data_ = new DataConnection(Config.Provider, Config.ConnectionString);

                //db.dataMapper = this;
                if (DatabaseModel.State < StateMode.Running)
                {
                    Debug.WriteLine("A carregar banco de dados...");
                    Debug.WriteLine("DROOM.MVC");
                    Debug.WriteLine("A mapear objectos...");
                    
                   var instance = DatabaseModel.Instance;

                    //db.GetConnection.ConnectionString = DatabaseConsts.ConnectionString;

                    Debug.WriteLine("");
                    Debug.WriteLine("Objectos encontrados:");
                    Debug.WriteLine("Tabelas: " + string.Join("\n", instance.Tables.Select(a => a.Value.Name).ToArray()));
                    Debug.WriteLine("View: " + string.Join("\n", instance.Views.Select(a => a.Value.Name).ToArray()));
                    Debug.WriteLine("Procedures: ");
                    
                    if (DatabaseModel.State==StateMode.Running)
                    {                        
                        if (Generator.Commands.Database.CreateDatabaseInServer(this))
                        {
                            //foreach (var db_name in instance.Databases)
                            //{
                            //    //DatabaseConsts.ActiveBD = db_name.Key;

                            //    //if (IsExecute(DatabaseConsts.ActiveBD, AutoAction.Type.CreateDatabase) && IsExecute(DatabaseConsts.ActiveBD, AutoAction.Type.WriteData) == false)
                            //    {
                            //        this.Connection.StartTransaction();

                            //        Save(new Components { DatabaseModel = System.Text.Json.JsonSerializer.Serialize(DatabaseModel.Instance) }).Validate();

                            //        foreach (var item in Reflection.Database.GetInitializes())
                            //        {
                            //            item.dataMapperInternal = this;
                            //            item.Initialize();
                            //        }
                            //        CORE.MVC.Log.Write("'" + db_name.Key + "' a gravar dados");

                            //        //Save(AutoAction.Instance(DatabaseConsts.ActiveBD, AutoAction.Type.WriteData)).Validate();
                            //        this.Connection.Commit();
                            //    }
                            //}
                        }                        
                        DatabaseModel.SetStatus(StateMode.Ready);
                        Debug.WriteLine("Concluído");
                        CORE.MVC.Log.Write("Concluído");
                    }
                    Data.Connection.ChangeDatabase(DatabaseConsts.Master);
                }
                else
                {
                    ////Data.Connection.ChangeDatabase(DatabaseConsts.Master);
                }
                Data.Connection.ChangeDatabase(Config.DatabaseName);

            }
            catch (Exception ex)
            {
                DatabaseModel.SetStatus(StateMode.Error);
                if (this.Connection.UseTransaction)
                {
                    this.Connection.Rollback();
                }
                CORE.MVC.Log.Write(ex.Message);
                //Debug.Fail(ex.Message);
                throw ex;
            }
        }
        internal bool IsExecute(string Database,AutoAction.Type Tipo)
        {
            //return Find.Exists<AutoAction>(i =>i.DbName==Database && i.Tipo == Tipo && i.DataExec.HasValue);
            return true;
        }
        #endregion

        #region Cud

        #region Save
        /// <summary>
        /// Salva os dados no banco 
        /// </summary>
        /// <param name="model">Entity a ser salvo</param>
        /// <param name="cascade">Entity relacionadas</param>
        /// <returns>Devolve um objeto do tipo 'Result'</returns>
        public Result Save(Entity model, bool cascade = true)
        {   
            var rs = new Result();
            //Entity model = entity.Clone();
            Entity modelCatch = model.Clone();
            var comit = IsMultiplyTransation == false;

            try
            {
                var type = model.GetType();

                if (cascade && Connection.GlobalTransaction==false && Connection.UseTransaction==false){
                    Connection.StartInternalTransaction();                    
                }

                var Table = DatabaseModel.Instance.Tables.GetTable(type);
                if (model.IsChanged || model.RowNumber > 0)
                {
                    model.db_ = TriggerDataMaper();

                    if (model.Validation.IsValid == false)
                    {
                        rs.AddError(model.Validation.Errors);
                        throw new Exception("throw-validations");
                    }
                    SaveAux(model, cascade, Table);
                    //model.PreserveState();
                }else{

                    throw new Exception("Não foram encontradas mudanças para salvar.");
                }
                //UpdateTransationID();

                if (comit && Connection.GlobalTransaction == false && Connection.UseTransaction){

                    Connection.Commit();
                }
            }
            catch (Exception ex)
            {
                ex = ex.InnerException == null ? ex : ex.InnerException;
                if (Validation.IsUniqueError(ex)==false && ex.Message != "throw-validations")
                {
                    rs.AddError(ex);
                }
                if (Validation.IsUniqueError(ex))
                {
                    var field = Validation.GetUniqueError(ex);

                    rs.AddError(field.Error);
                }
                if (model != null)
                {
                    model.IsManipulated = false;
                    try
                    {
                        model.CallMedthod("TriggerException");
                    }
                    catch (Exception ex_fk)
                    {
                    }
                }

                if (comit && Connection.GlobalTransaction == false && Connection.UseTransaction)
                {
                    Connection.Rollback();
                }
                model.Set(modelCatch);
            }
            return rs;
        }
        /// <summary>
        /// Salva a coleção no dados no banco 
        /// </summary>
        /// <param name="model">ICollection a ser salvo</param>
        /// <param name="cascade">Entity relacionadas</param>
        /// <returns>Devolve um objeto do tipo 'Result'</returns>
        public Result Save(IEnumerable<Entity> models, bool cascade = true)
        {
            var rs = new Result();
            var comit = IsMultiplyTransation == false;

            List<Entity> Models = new List<Entity>(models.Count());
            try
            {
                for (int i = 0; i < models.Count(); i++)
                {
                    Models.Add(models.ElementAt(i).Clone());
                }
                if (cascade && Connection.GlobalTransaction == false && Connection.UseTransaction == false)
                {
                    Connection.StartInternalTransaction();
                }
                for (int i = 0; i < models.Count(); i++)
                {
                    Entity model = models.ElementAt(i);
                    if (model != null && model.IsChanged)
                    {
                        model.db_ = TriggerDataMaper();

                        if (model.Validation.IsValid == false)
                        {
                            rs.AddError(model.Validation.Errors);
                        }
                    }else{
                        throw new Exception($"Não foram encontradas mudanças para salvar.{model.GetType().Name}[{i}]");
                    }
                }
                if (rs.Success == false)
                {
                    throw new Exception("throw-validations");
                }
                for (int i = 0; i < models.Count(); i++)
                {                
                Entity model = models.ElementAt(i);
                    var Table = DatabaseModel.Instance.Tables.GetTable(model.GetType());
                    if (model.IsChanged || (model.RowNumber > 0 && Table.Fks.Count > 0))
                    {
                    SaveAux(model, cascade, Table);
                    //model.PreserveState();
                    }
                }

                if (comit && Connection.GlobalTransaction == false && Connection.UseTransaction)
                {
                    Connection.Commit();
                }
            }
            catch (Exception ex)
            {
                ex = ex.InnerException == null ? ex : ex.InnerException;

                if (Validation.IsUniqueError(ex) == false && ex.Message != "throw-validations")
                {
                    rs.AddError(ex);
                }
                if (Validation.IsUniqueError(ex))
                {
                    var field = Validation.GetUniqueError(ex);

                    rs.AddError(field.Error);
                }
                for (int i = 0; i < models.Count(); i++)
                {
                    Entity model = models.ElementAt(i);
                    try
                    {
                        if (model != null && model.IsManipulated)
                        {
                            model.IsManipulated = false;
                            model.CallMedthod("TriggerException");
                        }
                    }
                    catch (Exception ex_fk)
                    {
                    }
                    if(model != null)
                    model.Set(Models[i]);
                }
                if (comit && Connection.GlobalTransaction == false && Connection.UseTransaction)
                {
                    Connection.Rollback();
                }
               
            }
            return rs;
        }

        #region Models - Metodos
        private bool GetSaveAuxInsertOrUpdate(object model)
        {
            var tb = DatabaseModel.Instance.Tables.GetTable(model.GetType());
            try
            {
                return Connection.Execute<bool>($"SELECT CONVERT(BIT, 1) FROM {tb.Name} (nolock) WHERE {tb.PrimaryKey.Name} = @val", new DataParameter("val", model.GetValueProperty(tb.PrimaryKey.Property.Name)));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Localiza objetos relacionados e salva-os
        /// </summary>
        /// <param name="model"></param>
        /// <param name="Parentkey"></param>
        private void SaveForeignAux(object model,bool cascade_save ,bool Parentkey){
            
        if (cascade_save)
            {
                var Table = DatabaseModel.Instance.Tables.GetTable(model.GetType());

                var fks = Parentkey ? Table.ForeignKeyParents() : Table.ForeignKeyChields();
                //foreach (var item in Table.Fks.Where(i => (Parentkey == false && string.IsNullOrWhiteSpace(i.Value.Fields.ParentKey)) || (Parentkey && !string.IsNullOrWhiteSpace(i.Value.Fields.ParentKey))).ToList())
                foreach (var item in fks.Where(i=>i.Value.IsNotSave==false).ToList())
                {
                    var prop = model.GetType().GetProperty(item.Key);
                    var tbFK = DatabaseModel.Instance.Tables.GetTable(item.Value.TypeModel);

                    object val = prop.GetValue(model);

                    if (prop.IsCollections() && val != null)
                    {
                        object[] list = ((IEnumerable<object>)val).ToArray();
                        
                        for (int i = 0; i < list.Length; i++)
                        {   
                            var mod = list[i] as Entity;
                            mod.db_ = this;
                            var type = list[i].GetType();
                            if (mod!=null && mod.IsChanged)
                            {
                                setDefaultValues(list[i], item.Value.Fields);

                                //if (string.IsNullOrWhiteSpace(item.Value.Fields.ParentKey) == false)
                                if (item.Value.Fields.IsChield)
                                {
                                    list[i].SetValueProperty(item.Value.Fields.ParentKey, model.GetValueProperty(item.Value.Fields.ForeignKey));
                                }
                                if (mod.Validation.IsValid == false)
                                {
                                    throw new Exception(mod.Validation.Error);
                                }
                                SaveAux(list[i], cascade_save, tbFK);
                                if (item.Value.Fields.IsChield==false)
                                {
                                    model.SetValueProperty(item.Value.Fields.ForeignKey, list[i].GetValueProperty(tbFK.PrimaryKey.Property.Name));
                                }
                            }
                        }
                    }
                    else if (val != null)
                    {
                        var mod = val as Entity;
                        mod.db_ = this;
                        if (mod.IsChanged)
                        {
                            setDefaultValues(val, item.Value.Fields);
                            //if (item.Value.Fields.IsChield && tbFK.PrimaryKey.AutoIncrement.HasValue)
                            if (item.Value.Fields.IsChield)
                            {
                                val.SetValueProperty(item.Value.Fields.ParentKey, model.GetValueProperty(item.Value.Fields.ForeignKey));
                            }
                            if (mod.Validation.IsValid == false)
                            {
                                throw new Exception(mod.Validation.Error);
                            }
                            SaveAux(val, cascade_save, tbFK);
                            //mod.PreserveState();
                            //if (item.Value.Fields.IsChield == false)
                            if (item.Value.Fields.IsChield==false)
                            {
                                model.SetValueProperty(item.Value.Fields.ForeignKey, val.GetValueProperty(tbFK.PrimaryKey.Name));
                            }
                        }
                    }
                }
            }
        }
        List<string> tbNameList = new List<string>();
        void setDefaultValues(object model,FkAttribute fk){
            if(model!=null && fk!=null && fk.DefaultValues!=null){
                foreach (var item in fk.DefaultValues)
                {   
                    model.SetValueProperty(item.Key, item.Value);
                }
            }
        }
        void SetTbNameTransactionID(string name){
            //if(tbNameList.Contains(name) == false){
            //    tbNameList.Add(name);
            //}
        }
        private void SaveAux(object model, bool cascade, DatabaseModel.Table table)
        {
            var type = model.GetType();
            var entity = ((Entity)model);
            bool IsChanged = entity.IsChanged;
            
            if (entity.RowNumber > 0 || IsChanged)
            {
                ((Entity)model).IsManipulated = true;

                //((Entity)model).TransactionID = Connection.TransactionID;
                //Salva foreign key parent
                SaveForeignAux(model, cascade, true);

                bool rs = entity.RowNumber > 0; //GetSaveAuxInsertOrUpdate(model);
                if (IsChanged)
                {   
                    if (rs == false)
                    {
                        InsertAux(model, table);
                    }
                    else
                    {
                        UpdateAux(model, table);
                    }
                    ((Entity)model).PreserveState();
                }
                //Salva foreign key chield
                SaveForeignAux(model, cascade, false);
            }
            //else{
            //    throw new Exception("Action não especificado.");
            //}
        }
        private void InsertAux(object model,DatabaseModel.Table table)
        {
            model.CallMedthod("TriggerBeforeInsert");
            model.CallMedthod("TriggerBeforeSave");            
            if (table.PrimaryKey.AutoIncrement==Entity.PK.Application)
            {
                model.SetValueProperty(table.PrimaryKey.Name, Generation.ID);
            }

            StringBuilder query = new StringBuilder($"INSERT INTO {table.Name} ");
            List<DataParameter> parameters = new List<DataParameter>();

            query.Append("(");
            //query.Append(string.Join(",", table.Columns.Select(i => i.Name).Where(i => (table.PrimaryKey.AutoIncrement == Entity.PK.Database && i != table.PrimaryKey.Name) || table.PrimaryKey.AutoIncrement == Entity.PK.Application)));
            query.Append(string.Join(",", table.Columns.Where(i => i.AutoIncrement.HasValue == false || (i.AutoIncrement != Entity.PK.Database)).Select(i => i.Name).ToList()));
            query.Append(")");
            //TransactionID
            List<string> values = new List<string>();

            //foreach (var col in table.Columns.Where(i=>( table.PrimaryKey.AutoIncrement == Entity.PK.Database /*&& i.Name!=table.PrimaryKey.Name*/) || table.PrimaryKey.AutoIncrement == Entity.PK.Application).ToList())

            foreach (var col in table.Columns.Where(i => i.Name != "TransactionID" && (i.AutoIncrement.HasValue==false ||(i.AutoIncrement!=Entity.PK.Database))).ToList())
            {
                object val = model.GetValueProperty(col.Property.Name);
                if (val != null)
                {
                    values.Add($"@{col.Name}");                   

                    parameters.Add(new DataParameter
                    {
                        Name = col.Name,
                        //SqlDbType = col.Type,
                        Value = val ?? DBNull.Value
                    });
                    
                }else{
                    values.Add($"NULL");
                }
            }
            //values.Add($"@TransactionID");
            //parameters.Add(new DataParameter
            //{
            //    ParameterName = "TransactionID",
            //    SqlDbType = System.Data.SqlDbType.BigInt,
            //    Value = Connection.TransactionID.HasValue ? (object)Connection.TransactionID.Value : DBNull.Value
            //});
            query.Append("VALUES(");
            query.Append(string.Join(",", values));
            query.AppendLine(");");

            if (table.PrimaryKey.AutoIncrement==Entity.PK.Database)
            {
                query.AppendLine("SELECT @@identity AS [ID]");//@@SCOPE_IDENTITY()
                object val = Connection.Execute<object>(query.ToString(), parameters.ToArray());
                model.SetValueProperty(table.PrimaryKey.Property.Name, Convert.ChangeType(val, table.PrimaryKey.Property.PropertyType));
            }
            else
            {
                Connection.Execute(query.ToString(), parameters.ToArray());
            }
            SetTbNameTransactionID(table.Name);
            int RowNumber = Connection.Execute<int>($"SELECT COUNT({table.PrimaryKey.Name}) FROM {table.Name} (NOLOCK)");

            model.SetValueProperty("RowNumber", RowNumber);
            //model.SetValueProperty("Action", Entity.Config.Action.None);
            model.CallMedthod("TriggerAfterInsert");
            model.CallMedthod("TriggerAfterSave");
        }
        private void InsertAuxTemp(object pk, DatabaseModel.Table table)
        {
            //StringBuilder query = new StringBuilder($"INSERT INTO {table.TempName} ");
            //List<DataParameter> parameters = new List<DataParameter>();
            //var cols = table.Columns.Select(i => i.Name).ToList();
            //query.Append("(");
            //query.Append(string.Join(",", cols));
            //query.Append(")");
            //query.Append("SELECT ");
            ////TransactionID
            //List<string> values = new List<string>();

            //foreach (var item in cols)
            //{
            //    if("TransactionID" == item){
            //        values.Add($"{Connection.TransactionID.GetValueOrDefault()}");
            //    }else{
            //        values.Add($"{item}");
            //    }
            //}
            //query.Append(string.Join(",", values));
            //query.Append($" FROM {table.Name} A (READPAST) WHERE A.{table.PrimaryKey.Name} = @{table.PrimaryKey.Name} AND ");
            //query.Append($"NOT EXISTS(SELECT 1 FROM {table.TempName} T (NOLOCK) WHERE T.{table.PrimaryKey.Name} = @{table.PrimaryKey.Name}) ");

            //parameters.Add(new DataParameter
            //{
            //    ParameterName = table.PrimaryKey.Name,
            //    SqlDbType = table.PrimaryKey.Type,
            //    Value = pk
            //});
            //var rs =DataMapper.Instance.Connection.Execute(query.ToString(), parameters.ToArray());
        }

        private void UpdateAux(object model, DatabaseModel.Table table)
        {          
            var colState = ((Entity)model).SearchStateChange();
            if(colState.Count ==0 ){ return; }

            model.CallMedthod("TriggerBeforeUpdate");
            model.CallMedthod("TriggerBeforeSave");
            object pk_val = model.GetValueProperty(table.PrimaryKey.Property.Name);

            //InsertAuxTemp(pk_val, table);

            List<DataParameter> parameters = new List<DataParameter>();
            StringBuilder query = new StringBuilder($"UPDATE {table.Name} ");

            query.Append("SET ");

            List<string> values = new List<string>();
            foreach (var item in colState)
            {
                var col = table.Columns.FirstOrDefault(i => i.Property.Name == item.Key);
                //foreach (var col in table.Columns.Where(i => i.PrimaryKey == false).ToList())
                {
                    object val = model.GetValueProperty(col.Property.Name);
                    if (val != null)
                    {
                        values.Add($"{col.Name} = @{col.Name}");
                        parameters.Add(new DataParameter
                        {
                            Name = col.Name,
                            //SqlDbType = col.Type,
                            Value = val ?? DBNull.Value
                        });
                        
                    }
                    else
                    {
                        values.Add($"{col.Name} = NULL");
                    }
                }

            }

            //values.Add($"TransactionID = @TransactionID");

            //parameters.Add(new DataParameter
            //{
            //    ParameterName = "TransactionID",
            //    SqlDbType = System.Data.SqlDbType.BigInt,
            //    Value = Connection.TransactionID.HasValue ? (object)Connection.TransactionID.Value : DBNull.Value
            //});

            query.Append(string.Join(",", values));
            query.AppendLine($" WHERE {table.PrimaryKey.Name} = @{table.PrimaryKey.Name}");

            parameters.Add(new DataParameter
            {
                Name = table.PrimaryKey.Name,
                //SqlDbType = table.PrimaryKey.Type,
                Value = pk_val
            });
            var sq = query.ToString();
            Connection.Execute(query.ToString(), parameters.ToArray());
            
            SetTbNameTransactionID(table.Name);

            //model.SetValueProperty("Action", Entity.Config.Action.None);

            model.CallMedthod("TriggerAfterUpdate");
            model.CallMedthod("TriggerAfterSave");
        }

        internal void UpdateTransationID()
        {            
            //List<DataParameter> parameters = new List<DataParameter>();
            //StringBuilder query = new StringBuilder();

            //foreach (var item in tbNameList)
            //{
            //    query.Append($"UPDATE {item} ");
            //    query.Append("SET ");
            //    query.Append($"TransactionID = NULL ");
            //    query.Append($"WHERE TransactionID IS NOT NULL AND TransactionID = {Connection.TransactionID.GetValueOrDefault()}; ");
            //}
            //if (tbNameList.Count > 0)
            //{
            //    var sq = query.ToString();
            //    Connection.Execute(query.ToString());

            //}
            //tbNameList.Clear();
        }
        internal void ResetTransationID()
        {            
            tbNameList.Clear();
        }

        #endregion

        #endregion

        #region Delete
        public enum DeleteAction
        {
            Temporary,
            Permanent
        }

        /// <summary>
        /// Elimina registos
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Chaves primarias</param>
        /// <returns></returns>
        public Result Delete<T>(params object[] key)
        {
            var result = new Result();
            var comit = IsMultiplyTransation == false;
            //try
            //{
            //    var table = DatabaseModel.Instance.Tables[typeof(T)];

            //    var list = Find.All<T>($"WHERE {table.PrimaryKey.Name} IN ({string.Join(",",key)})");

            //    if (list.Count == 0)
            //    {
            //        throw new Exception("Não foram encontrados registos para eliminar");
            //    }

            //    StringBuilder query = new StringBuilder($"UPDATE {table.Name} ");
            //    query.Append("SET Status=@Status");
            //    query.AppendLine($" WHERE {table.PrimaryKey.Name} = @{table.PrimaryKey.Name}");

            //    var col = table.Columns.FirstOrDefault(a => a.Name == "Status");
            //    if (Connection.GlobalTransaction == false && Connection.UseTransaction == false)
            //    {
            //        Connection.StartInternalTransaction();
            //    }

            //    for (int i = 0; i < list.Count; i++)
            //    {
            //    List<DataParameter> parameters = new List<DataParameter>();

            //    Entity model = list[i] as Entity;
            //    model.db_ = TriggerDataMaper();

            //    model.CallMedthod("TriggerBeforeDelete");

            //    parameters.Add(new DataParameter
            //    {
            //        ParameterName = "Status",
            //        SqlDbType = col.Type,
            //        Value = (int)Entity.State.Delete
            //    });
                    
            //    parameters.Add(new DataParameter
            //    {
            //        ParameterName = table.PrimaryKey.Name,
            //        SqlDbType = table.PrimaryKey.Type,
            //        Value = key[i]
            //    });
            //    //var sq = query.ToString();
            //    Connection.Execute(query.ToString(), parameters.ToArray());
            //    model.SetValueProperty(col.Property.Name, Entity.State.Delete);
            //    model.CallMedthod("TriggerAfterDelete");
            //    }

            //    if (comit && Connection.GlobalTransaction == false && Connection.UseTransaction)
            //    {
            //        Connection.Commit();
            //    }

            //}
            //catch (Exception ex)
            //{
            //    result.AddError(ex);
            //    if (comit && Connection.GlobalTransaction == false && Connection.UseTransaction)
            //    {
            //        Connection.Rollback();
            //    }
            //}
            return result;
        }

        /// <summary>
        /// Elimina registos em função de uma condição
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate">condição</param>
        /// <param name="Action">Acção</param>
        /// <returns></returns>
        public Result Delete<T>(Expression<Func<T, bool>> Where, DeleteAction Action = DeleteAction.Temporary)
        {
            var result = new Result();
            var comit = IsMultiplyTransation == false;
            //try
            //{
            //    var table = DatabaseModel.Instance.Tables[typeof(T)];

            //    var list = Find.All<T>(Where);

            //    if (list.Count == 0)
            //    {
            //        throw new Exception("Não foram encontrados registos para eliminar");
            //    }

            //    StringBuilder query = new StringBuilder();
            //    if(Action == DeleteAction.Temporary)
            //    {
            //        query.AppendLine($"UPDATE {table.Name} ");
            //        query.Append("SET Status=@Status");
            //    }
            //    else if (Action == DeleteAction.Permanent)
            //    {
            //        query.AppendLine($"DELETE FROM {table.Name} ");
            //    }
            //    query.AppendLine($" WHERE {table.PrimaryKey.Name} = @{table.PrimaryKey.Name}");

            //    var col = table.Columns.FirstOrDefault(a => a.Name == "Status");
            //    if (Connection.GlobalTransaction == false && Connection.UseTransaction == false)
            //    {
            //        Connection.StartInternalTransaction();
            //    }

            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        List<DataParameter> parameters = new List<DataParameter>();

            //        Entity model = list[i] as Entity;
            //        model.db_ = TriggerDataMaper();

            //        model.CallMedthod("TriggerBeforeDelete");

            //        if (Action == DeleteAction.Temporary)
            //        {
            //            parameters.Add(new DataParameter
            //            {
            //                ParameterName = "Status",
            //                SqlDbType = col.Type,
            //                Value = (int)Entity.State.Delete
            //            });
            //        }

            //        parameters.Add(new DataParameter
            //        {
            //            ParameterName = table.PrimaryKey.Name,
            //            SqlDbType = table.PrimaryKey.Type,
            //            Value = model.GetValueProperty(table.PrimaryKey.Property.Name)
            //        });
            //        //var sq = query.ToString();
            //        Connection.Execute(query.ToString(), parameters.ToArray());
            //        model.SetValueProperty(col.Property.Name, Entity.State.Delete);
            //        model.CallMedthod("TriggerAfterDelete");
            //    }

            //    if (comit && Connection.GlobalTransaction == false && Connection.UseTransaction)
            //    {
            //        Connection.Commit();
            //    }

            //}
            //catch (Exception ex)
            //{
            //    result.AddError(ex);
            //    if (comit && Connection.GlobalTransaction == false && Connection.UseTransaction)
            //    {
            //        Connection.Rollback();
            //    }
            //}
            return result;
        }

        public Result Delete(Type type,params object[] key)
        {
            var result = new Result();
            var comit = IsMultiplyTransation == false;
            //try
            //{
            //    var table = DatabaseModel.Instance.Tables[type];

            //    var list = Find.All(type,$"WHERE {table.PrimaryKey.Name} IN ({string.Join(",", key)})");

            //    if (list.Count == 0)
            //    {
            //        throw new Exception("Não foram encontrados registos para eliminar");
            //    }

            //    StringBuilder query = new StringBuilder($"UPDATE {table.Name} ");
            //    query.Append("SET Status=@Status");
            //    query.AppendLine($" WHERE {table.PrimaryKey.Name} = @{table.PrimaryKey.Name}");

            //    var col = table.Columns.FirstOrDefault(a => a.Name == "Status");
            //    if (Connection.GlobalTransaction == false && Connection.UseTransaction == false)
            //    {
            //        Connection.StartInternalTransaction();
            //    }

            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        List<DataParameter> parameters = new List<DataParameter>();

            //        Entity model = list[i] as Entity;
            //        model.db_ = TriggerDataMaper();

            //        model.CallMedthod("TriggerBeforeDelete");

            //        parameters.Add(new DataParameter
            //        {
            //            ParameterName = "Status",
            //            SqlDbType = col.Type,
            //            Value = (int)Entity.State.Delete
            //        });

            //        parameters.Add(new DataParameter
            //        {
            //            ParameterName = table.PrimaryKey.Name,
            //            SqlDbType = table.PrimaryKey.Type,
            //            Value = key[i]
            //        });
            //        //var sq = query.ToString();
            //        Connection.Execute(query.ToString(), parameters.ToArray());
            //        model.SetValueProperty(col.Property.Name, Entity.State.Delete);
            //        model.CallMedthod("TriggerAfterDelete");
            //    }

            //    if (comit && Connection.GlobalTransaction == false && Connection.UseTransaction)
            //    {
            //        Connection.Commit();
            //    }

            //}
            //catch (Exception ex)
            //{
            //    result.AddError(ex);
            //    if (comit && Connection.GlobalTransaction == false && Connection.UseTransaction)
            //    {
            //        Connection.Rollback();
            //    }
            //}
            return result;
        }

        #endregion

        #endregion

        #region Pesquisa

        ///// <summary>
        ///// Pesquisa e devolve um objecto
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key">Chave primaria</param>
        ///// <returns></returns>

        //public static Entity GetEntity(string TableName)
        //{
        //    var tb = DatabaseModel.Instance.Tables.FirstOrDefault(i => i.Key.FullName == TableName);
        //    return (Entity)Activator.CreateInstance(tb.Key);
        //}
        //public static Type GetEntityType(string TableName)
        //{
        //    var tb = DatabaseModel.Instance.Tables.FirstOrDefault(i => i.Key.FullName == TableName);
        //    return tb.Key;
        //}
        //public static Entity GetEntity(string TableName, string campo, object val)
        //{
        //    var tb = DatabaseModel.Instance.Tables.FirstOrDefault(i => i.Key.FullName == TableName);
        //    return new DataMapper().Find.First(tb.Key, $"{campo}=@val", new { val = val });
        //}
        //public static DataTable GetEntities(string TableName){
        //    var tb = DatabaseModel.Instance.Tables.FirstOrDefault(i => i.Key.FullName == TableName);
        //    return new DataMapper().Find.All(type:tb.Key);
        //}
        
        #endregion

        #region Extra
        internal bool IsMultiplyTransation { get { return Connection.UseTransaction; } }
        internal DataMapper TriggerDataMaper()
        {
            return this;
        }

        #endregion
    }

   

}
