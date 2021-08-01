using CodeCaster.SerializeThis.Serialization;
using EnvDTE;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public bool PopulateClassFromLocal(MemberInfo classInfo)
        {
            var stackFrame = _debugger.CurrentStackFrame;

            foreach (Expression local in stackFrame.Locals)
            {
                if (local.Name == classInfo.Name)
                {
                    PopulateClassInfo(classInfo, local);
                    return true;
                }
            }

            return false;
        }

        private void PopulateClassInfo(MemberInfo classInfo, Expression local)
        {
            if (classInfo.Class.Type != TypeEnum.ComplexType)
            {
                if (local.IsValidValue)
                {
                    var valueTypeValue = GetValueTypeValue(classInfo, local);
                    classInfo.Value = valueTypeValue;
                }

                return;
            }

            if (classInfo.Class.IsCollectionType)
            {
                classInfo.Value = CreateCollection(classInfo.Class, local);
                return;
            }

            // TODO: collection types
            // TODO: inheritance! Runtime can be different, update class? Find existing class by name?
            foreach (var prop in classInfo.Class.Children)
            {
                var localMember = FindMember(local, prop);
                PopulateClassInfo(prop, localMember);
            }
        }

        private IEnumerable CreateCollection(TypeInfo classInfo, Expression local)
        {
            IEnumerable<Expression> items = GetItemsFromCollection(local);
            return items.ToArray();
        }

        private IEnumerable<Expression> GetItemsFromCollection(Expression expression)
        {
            foreach (Expression member in expression.DataMembers)
            {
                yield return member;
            }
        }

        private Expression FindMember(Expression local, MemberInfo prop)
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

        private object GetValueTypeValue(MemberInfo classInfo, Expression local)
        {
            switch (classInfo.Class.Type)
            {
                case TypeEnum.Boolean:
                    return Boolean.Parse(local.Value);
                case TypeEnum.String:
                    return (String)local.Value == null || local.Value == "null" ? null : local.Value.Substring(1, local.Value.Length - 2);
                case TypeEnum.Byte:
                    return Byte.Parse(local.Value);
                case TypeEnum.Int16:
                    return Int16.Parse(local.Value);
                case TypeEnum.Int32:
                    return Int32.Parse(local.Value);
                case TypeEnum.Int64:
                    return Int64.Parse(local.Value);
                case TypeEnum.Float32:
                    return Single.Parse(local.Value);
                case TypeEnum.Float64:
                    return Double.Parse(local.Value);
                case TypeEnum.Decimal:
                    return Decimal.Parse(local.Value);
                case TypeEnum.DateTime:
                    return DateTime.Parse(local.Value);
                case TypeEnum.DateTimeOffset:
                    return DateTimeOffset.Parse(local.Value);
            }

            return null;
        }
    }
}