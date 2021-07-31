using CodeCaster.SerializeThis.Serialization;
using EnvDTE;
using System;

namespace CodeCaster.SerializeThis
{
    /// <summary>
    /// TODO.
    /// </summary>
    public class DebugValueParser
    {
        private readonly Debugger _debugger;

        public DebugValueParser(Debugger debugger)
        {
            _debugger = debugger;
        }

        internal void PopulateClassFromLocal(ClassInfo classInfo)
        {
            var stackFrame = _debugger.CurrentStackFrame;

            foreach (Expression local in stackFrame.Locals)
            {
                if (local.Name == classInfo.Name)
                {
                    PopulateClassInfo(classInfo, local);
                    break;
                }
            }
        }

        private void PopulateClassInfo(ClassInfo classInfo, Expression local)
        {
            if (classInfo.Class.Type != TypeEnum.ComplexType)
            {
                if (local.IsValidValue)
                {
                    var valueTypeValue = GetValueTypeValue(classInfo, local);
                    classInfo.Value = valueTypeValue?.ToString() != "null"
                        ? valueTypeValue
                        : null;
                }

                return;
            }

            // TODO: collection types
            foreach (var prop in classInfo.Class.Children)
            {
                var localMember = FindMember(local, prop);
                PopulateClassInfo(prop, localMember);
            }
        }

        private Expression FindMember(Expression local, ClassInfo prop)
        {
            foreach (Expression dataMember in local.DataMembers)
            {
                if (dataMember.Name == prop.Name)
                {
                    return dataMember;
                }
            }

            return null;
        }

        private object GetValueTypeValue(ClassInfo classInfo, Expression local)
        {
            switch (classInfo.Class.Type)
            {
                case TypeEnum.Boolean:
                case TypeEnum.String:
                case TypeEnum.Byte:
                case TypeEnum.Int16:
                case TypeEnum.Int32:
                case TypeEnum.Int64:
                case TypeEnum.Float32:
                case TypeEnum.Float64:
                case TypeEnum.Decimal:
                case TypeEnum.DateTime:
                case TypeEnum.DateTimeOffset:
                    return local.Value;
            }

            return null;
        }
    }
}