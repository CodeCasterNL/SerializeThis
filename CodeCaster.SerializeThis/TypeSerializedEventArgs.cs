using System;
using CodeCaster.SerializeThis.Serialization;

namespace CodeCaster.SerializeThis
{
    public class TypeSerializedEventArgs : EventArgs
    {
        public ClassInfo ClassInfo { get; set; }
        public string MenuItemName { get; set; }
        
        public TypeSerializedEventArgs(ClassInfo classInfo, string menuItemName)
        {
            this.ClassInfo = classInfo;
            this.MenuItemName = menuItemName;
        }
    }
}
