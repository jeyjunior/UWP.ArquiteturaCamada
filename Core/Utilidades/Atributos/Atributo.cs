using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilidades.Atributos
{
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class EditableAttribute : Attribute
    {
        public EditableAttribute(bool iseditable)
        {
            AllowEdit = iseditable;
        }

        public bool AllowEdit { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredAttribute : Attribute
    {
    }
}
