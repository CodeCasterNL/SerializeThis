using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCaster.SerializeThis.Serialization
{
    public class Node
    {
        public string Name { get; set; }

        public TypeEnum Type { get; set; }

        public bool IsCollection { get; set; }
    }
}
