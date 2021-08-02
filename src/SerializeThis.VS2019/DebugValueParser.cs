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
    public class DebugValueParser : IPropertyValueProvider
    {
        private class ExpressionInfo
        {
            public string Path { get; }

            public Expression Expression { get; }

            public TypeInfo Type { get; }

            public ExpressionInfo(string path, EnvDTE.Expression expression, TypeInfo type)
            {
                Path = path;
                Expression = expression;
                Type = type;
            }
        }

        private readonly Debugger _debugger;

        private Dictionary<string, ExpressionInfo> _valueDictionary;

        public DebugValueParser(Debugger debugger)
        {
            _debugger = debugger;
        }

        public bool CanHandle(TypeInfo declaredType, string name)
        {
            _valueDictionary = new Dictionary<string, ExpressionInfo>();
            
            var expression = FindLocalVariable(name);

            if (expression != null)
            {
                var info = new ExpressionInfo(name, expression, declaredType);
                _valueDictionary[name] = info;
                return true;
            }

            return false;
        }

        public MemberInfo Announce(MemberInfo toSerialize, string path)
        {
            if (!_valueDictionary.Any())
            {
                return null;
            }

            var expression = GetExpression(toSerialize, path);

            if (expression.Type != toSerialize.Class)
            {
                // TODO: polymorphism
                System.Diagnostics.Debugger.Break();
            }

            return toSerialize;
        }

        public object GetScalarValue(MemberInfo toSerialize, string path)
        {
            if (toSerialize.Class.IsComplexType)
            {
                return null;
            }

            // No local variables.
            if (!_valueDictionary.Any())
            {
                return null;
            }

            var expression = GetExpression(toSerialize, path);

            return GetValueTypeValue(expression);
        }

        private ExpressionInfo GetExpression(MemberInfo toSerialize, string path)
        {
            var parentPath = path;
            
            var lastDotIndex = path.LastIndexOf('.');
            if (lastDotIndex >= 0)
            {
                parentPath = parentPath.Substring(0, lastDotIndex);
            }

            if (!_valueDictionary.TryGetValue(parentPath, out var parentExpression))
            {
                // TODO: get expression type from event or something?
                throw new NotImplementedException($"TODO: look up type of parentPath '{parentPath}'");
                parentExpression = GetExpression(null, parentPath);
            }

            if (parentPath == path)
            {
                return parentExpression;
            }

            var currentExpression = FindMember(parentExpression, toSerialize);
            
            return _valueDictionary[path] = new ExpressionInfo(path, currentExpression, toSerialize.Class);
        }

        // TODO: collections (and syntax, [i]?)
        public IEnumerable<object> GetCollectionElements(MemberInfo child, string path, MemberInfo collectionType)
        {
            throw new NotImplementedException();
        }

        public Expression FindLocalVariable(string localName)
        {
            var stackFrame = _debugger.CurrentStackFrame;

            foreach (Expression local in stackFrame.Locals)
            {
                if (local.Name == localName)
                {
                    return local;
                }
            }

            return null;
        }

        //private void PopulateClassInfo(MemberInfo classInfo, Expression local)
        //{
        //    if (classInfo.Class.Type != TypeEnum.ComplexType)
        //    {
        //        if (local.IsValidValue)
        //        {
        //            var valueTypeValue = GetValueTypeValue(new ExpressionInfo()classInfo.Class, local);
        //            classInfo.Value = valueTypeValue;
        //        }

        //        return;
        //    }

        //    if (classInfo.Class.IsCollectionType)
        //    {
        //        classInfo.Value = CreateCollection(classInfo.Class, local);
        //        return;
        //    }

        //    // TODO: collection types
        //    // TODO: inheritance! Runtime can be different, update class? Find existing class by name?
        //    foreach (var prop in classInfo.Class.Children)
        //    {
        //        var localMember = FindMember(local, prop);
        //        PopulateClassInfo(prop, localMember);
        //    }
        //}

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

        private Expression FindMember(ExpressionInfo member, MemberInfo prop)
        {
            foreach (Expression dataMember in member.Expression.DataMembers)
            {
                if (dataMember.Name == prop.Name)
                {
                    return dataMember;
                }
            }

            return null;
        }

        private object GetValueTypeValue(ExpressionInfo expressionInfo)
        {
            if (expressionInfo == null)
            {
                return null;
            }

            var value = expressionInfo.Expression.Value;

            switch (expressionInfo.Type.Type)
            {
                case TypeEnum.Boolean:
                    return Boolean.Parse(value);

                case TypeEnum.String:
                    return (String)value == null || value == "null" ? null : value.Substring(1, value.Length - 2);

                case TypeEnum.Byte:
                    return Byte.Parse(value);

                case TypeEnum.Int16:
                    return Int16.Parse(value);

                case TypeEnum.Int32:
                    return Int32.Parse(value);

                case TypeEnum.Int64:
                    return Int64.Parse(value);

                case TypeEnum.Float32:
                    return Single.Parse(value);

                case TypeEnum.Float64:
                    return Double.Parse(value);

                case TypeEnum.Decimal:
                    return Decimal.Parse(value);

                case TypeEnum.DateTime:
                    return DateTime.Parse(value);

                case TypeEnum.DateTimeOffset:
                    return DateTimeOffset.Parse(value);
            }

            return null;
        }
    }
}