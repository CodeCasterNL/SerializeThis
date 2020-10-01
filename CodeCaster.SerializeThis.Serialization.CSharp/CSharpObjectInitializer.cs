using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCaster.SerializeThis.Serialization.CSharp
{
    public class CSharpObjectInitializer : IClassInfoSerializer
    {
        public string FileExtension => "cs";

        public string Serialize(ClassInfo type)
        {
            throw new NotImplementedException();
        }
    }
}
