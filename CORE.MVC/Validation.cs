using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CORE.MVC
{
   public class Validation
    {
        object model = null;

        List<string> Errors_ = new List<string>();
        List<Field> ErrorFields_ = new List<Field>();

        private void CallExiste()
        {
            try
            {

                var entity = model as Entity;
                var shema = DatabaseModel.Instance.Tables[model.GetType()];
                var list = model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(i => i.GetCustomAttribute<ExistAttribute>() != null).ToList();
                DataTable dt = new DataTable();
                
                foreach (var item in list)
                {
                    object val = model.GetValueProperty(item.Name);
                    object id = model.GetValueProperty(shema.PrimaryKey.Property.Name) ?? 0;

                    if (val != null && val != (object)"")
                    {
                        if (item.PropertyType == typeof(string))
                        {
                           // dt = entity.db_.Find.DataTable(model.GetType(), new string[] { $"{shema.PrimaryKey.Property.Name}" }, $"WHERE {shema.PrimaryKey.Property.Name} <> @id AND LOWER({item.GetColumn()}) = LOWER(@chave) and Status=1", new { chave = val, id = id });
                        }
                        else
                        {
                           // dt = entity.db_.Find.DataTable(model.GetType(), new string[] { $"{shema.PrimaryKey.Property.Name}" }, $"WHERE {shema.PrimaryKey.Property.Name} <> @id AND {item.GetColumn()} = @chave and Status = 1", new { chave = val, id = id });
                        }
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            var prop = shema.Columns.FirstOrDefault(i => i.Property.Name == item.Name);
                            AddError($"{prop.Display} {val ? .ToString()} já existe");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

        /// <summary>
        /// Verifica se o modelo está valido
        /// </summary>
        public bool IsValid { get {
                LoadValidations();
                model.CallMedthod("SetValidations");
                if (Errors_.Count == 0) { CallExiste(); }
                var valid = Errors_.Count == 0; return valid;
            } }

        /// <summary>
        /// Lista de erros
        /// </summary>
        public string[] Errors { get { return Errors_.ToArray(); } }
        public string Error { get { return Errors_[0] ?? ""; } }
        /// <summary>
        /// Lista de mensagens com os respectivos campos
        /// </summary>
        public Field[] Fields { get { return ErrorFields_.ToArray(); } }

        /// <summary>
        /// Mensagem de erro
        /// </summary>
        /// <param name="error"></param>
        public void AddError(string error)
        {
            Errors_.Add(error);
        }
        /// <summary>
        /// Adicionada um erro associado a um campo
        /// </summary>
        /// <param name="field">Variavel a atribuir o erro</param>
        /// <param name="error">Descrição de erro</param>
        public void AddError(string field, string error)
        {
            ErrorFields_.Add(new Field { Name=field, Error = error });
            AddError(error);
        }

        public Validation(Entity model){
            this.model = model;
        }

        private void LoadValidations(){
            ErrorFields_.Clear();
            Errors_.Clear();
        }

        public class Field{
            public string Name { get; set; }
            public string Error { get; set; }
        }
        internal static bool IsUniqueError(Exception ex)
        {
            return ex.Message.Contains("UNIQUE KEY");
        }
        internal static Field GetUniqueError(Exception ex)
        {
            if (Validation.IsUniqueError(ex))
            {
                var unique = ex.Message.Split('_')[2].Split('\'')[0];
                //DatabaseModel.Instance.Tables[type].g
                var field = new Field
                {
                    Name=unique,
                    Error=$"O valor do campo '{unique}' já existe"
                };
                return field;
            }
            return null;
        }
    }
}
