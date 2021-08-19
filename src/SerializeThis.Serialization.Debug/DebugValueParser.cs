using EnvDTE;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SerializeThis.Serialization.Debug
{
    /// <summary>
    /// TODO.
    /// </summary>
    public partial class DebugValueParser : IPropertyValueProvider
    {
        private readonly Debugger _debugger;
        private readonly ITypeInfoProvider _typeInfoProvider;
        private Dictionary<string, ExpressionInfo> _valueDictionary;

        public DebugValueParser(Debugger debugger, ITypeInfoProvider typeInfoProvider)
        {
            _debugger = debugger;
            _typeInfoProvider = typeInfoProvider;
        }

        public bool CanHandle(TypeInfo declaredType, string path)
        {
            _valueDictionary = new Dictionary<string, ExpressionInfo>();

            var expression = FindLocalVariable(path);

            if (expression != null)
            {
                var runtimeType = GetRuntimeType(declaredType, expression);
                
                var info = new ExpressionInfo(path, expression, runtimeType);
                
                _valueDictionary[path] = info;
                
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

            // Cache the current expression.
            _ = GetExpression(toSerialize, path);

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

            var currentExpression = parentExpression.FindMember(toSerialize);

            var runtimeType = GetRuntimeType(toSerialize.Class, currentExpression);

            return _valueDictionary[path] = new ExpressionInfo(path, currentExpression, runtimeType);
        }

        private TypeInfo GetRuntimeType(TypeInfo typeInfo, Expression expression)
        {
            var typeName = expression.GetTypeName();
            if (typeName != typeInfo.TypeName)
            {
                // TODO: SymbolFinder?
                var runtimeType = _typeInfoProvider.GetTypeInfo(expression.Type);
                if (runtimeType != null)
                {
                    return runtimeType;
                }
            }

            return typeInfo;
        }

        // TODO: collections (and syntax, [i]?)
        public IEnumerable<object> GetCollectionElements(MemberInfo child, string path, MemberInfo collectionType)
        {
            throw new NotImplementedException();
        }

        public Expression FindLocalVariable(string localName)
        {
            var stackFrame = _debugger.CurrentStackFrame;

            if (stackFrame == null)
            {
                return null;
            }

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
                    return bool.Parse(value);

                case TypeEnum.Char:
                    return char.Parse(value);

                case TypeEnum.String:
                    return value == null || value == "null" ? null : value.Substring(1, value.Length - 2);

                case TypeEnum.Byte:
                    return byte.Parse(value);

                case TypeEnum.Int16:
                    return short.Parse(value);

                case TypeEnum.Int32:
                    return int.Parse(value);

                case TypeEnum.Int64:
                    return long.Parse(value);

                case TypeEnum.Float32:
                    return float.Parse(value);

                case TypeEnum.Float64:
                    return double.Parse(value);

                case TypeEnum.Decimal:
                    return decimal.Parse(value);

                case TypeEnum.DateTime:
                    return DateTime.Parse(value);

                case TypeEnum.DateTimeOffset:
                    return DateTimeOffset.Parse(value);
            }

            return null;
        }
    }
}