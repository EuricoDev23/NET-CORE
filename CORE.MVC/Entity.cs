using LinqToDB;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CORE.MVC
{
    /// <summary>
    /// Representa uma tabela da base de dados
    /// </summary>
    public class Entity
    {
        [NotMapped]
        internal DataMapper db_ = null; //DatabaseModel.DataMapper;
        [NotMapped]
        internal Validation Validation_ = null;
        [Column,NotNull]
        public DateTime DateCreated { get; set; } = DateTime.Now;
        /// <summary>
        /// Estado do registo na base de dados
        /// </summary>
        //public int Status { get; set; } = State.Active.Value();
        [Column, NotNull]
        public State Status { get; set; } = State.Active;

        [NotMapped]
        //[Browsable(false)]
        internal int RowNumber { get; set; }

        [NotMapped]
        internal bool IsChanged { get {
            var rs = SearchStateChange().Count > 0;
                var tb = DatabaseModel.Instance.Tables.GetTable(GetType()).Fks.Count;
            return rs || tb > 0;
        } }
        [NotMapped]
        internal bool IsManipulated = false;
        [NotMapped]
        Dictionary<string, object> StateFields = new Dictionary<string, object>();
        public Entity() {        
            PreserveState();
        }
        public bool Changed(){
            return IsChanged;
        }
        public Models.AutoNumberLog Generate(Models.AutoNumber.Type type)
        {
            return Models.AutoNumber.Generate(GetType(), type);
        }
        /// <summary>
        /// Salva os dados no banco 
        /// </summary>
        /// <param name="cascade">Entity relacionadas</param>
        /// <returns>Devolve um objeto do tipo 'Result'</returns>
        public Result Save(bool cascade = true)
        {
            return DataMapper.Save(this,cascade);
        }
        [NotMapped]
        public Validation Validation { 
            get
            {
                Validation_= Validation_ ?? new Validation(this);

                return Validation_;
            } 
        }
        [NotMapped]
        protected DataMapper DataMapper
        {
            get
            {               
                return db_ ?? new DataMapper();
            }
        }

        public Entity Clone(){
            return this.MemberwiseClone() as Entity;
        }

        #region State Field
        internal void PreserveState(){
            StateFields.Clear();
            foreach (var item in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {   
                if (item.CustomAttributes.Any(a => a.AttributeType == typeof(ColumnAttribute)))
                //if (item.CustomAttributes.Any(a => a.AttributeType != typeof(NotMappedAttribute)))
                {
                    StateFields.Add(item.Name, this.GetValueProperty(item.Name));
                }
            }
        }
        internal Dictionary<string,object> SearchStateChange()
        {
            Type type = GetType();
            Dictionary<string, object> tmp = new Dictionary<string, object>();
            foreach (var item in StateFields)
            {   
                object val = type.GetProperty(item.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(this);
                if (Equals(val,item.Value)== false /*&& item.Key != "TransactionID"*/)
                {
                    tmp.Add(item.Key, val);
                }
            }
            return tmp;
        }
        #endregion

        #region Custom
        /// <summary>
        /// Validações personalizadas
        /// </summary>
        protected virtual void SetValidations()
        {

        }
        #endregion

        #region Trigger
        /// <summary>
        /// Evento de gatilho disparado quando falha
        /// </summary>
        protected virtual void TriggerException()
        {

        }
        /// <summary>
        /// Evento de gatilho disparado depois de inserir
        /// </summary>
        protected virtual void TriggerAfterInsert()
        {

        }
        /// <summary>
        /// Evento de gatilho disparado depois de salvar
        /// </summary>
        protected virtual void TriggerAfterSave()
        {

        }
        /// <summary>
        /// Evento de gatilho antes de salvar
        /// </summary>
        protected virtual void TriggerBeforeSave()
        {

        }
        /// <summary>
        /// Evento de gatilho disparado antes de inserir
        /// </summary>
        protected virtual void TriggerBeforeInsert()
        {

        }
        /// <summary>
        /// Evento de gatilho disparado antes de atualizar
        /// </summary>
        protected virtual void TriggerBeforeUpdate()
        {

        }
        /// <summary>
        /// Evento de gatilho disparado depois de atualizar
        /// </summary>
        protected virtual void TriggerAfterUpdate()
        {

        }
        /// <summary>
        /// Evento de gatilho disparado antes de Delete
        /// </summary>
        protected virtual void TriggerBeforeDelete()
        {

        }
        /// <summary>
        /// Evento de gatilho disparado depois de Delete
        /// </summary>
        protected virtual void TriggerAfterDelete()
        {

        }
        /// <summary>
        /// Evento de gatilho ao pesquisar
        /// </summary>
        protected virtual void TriggerSearch()
        {

        }
        /// <summary>
        /// Evento de log
        /// </summary>
        protected virtual void Log()
        {
            
        }
        #endregion

        #region Enums
        public enum PK
        {
            Application = 1,
            Database = 2
        }
        public enum OnDelete
        {
            Default,
            Cascade,
            SetNull
        }
        
        public enum State
        {
            Active = 1,
            Delete = 0,
            Error = -1,
            Inactive = 3,
            Test = 2,
            Admin = 4
        }

        #endregion

        #region Pesquisa
       
        #endregion
    }
}
