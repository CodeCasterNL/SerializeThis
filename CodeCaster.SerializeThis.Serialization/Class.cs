using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCaster.SerializeThis.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    public class Class
    {
        public string Name { get; set; }

        public TypeEnum Type { get; set; }

        public bool IsNullable { get; set; }

        public bool IsCollection { get; set; }

        public IList<Class> Children { get; set; }
    }
}
