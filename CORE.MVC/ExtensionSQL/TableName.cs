using System;

namespace CORE.MVC.ExtensionSQL
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableName : Attribute, IAttributeName
    {
        private string name;

        public TableName(string name)
        {
            this.name = name;
        }


        public string GetName()
        {
            return name;
        }
    }
}