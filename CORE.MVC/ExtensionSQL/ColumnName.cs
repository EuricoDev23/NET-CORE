using System;

namespace CORE.MVC.ExtensionSQL
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnName : Attribute, IAttributeName
    {
        public ColumnName(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string GetName()
        {
            return Name;
        }
    }
}