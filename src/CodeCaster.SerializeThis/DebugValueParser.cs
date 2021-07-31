using CodeCaster.SerializeThis.Serialization;
using EnvDTE;
using System;
using System.Collections.Generic;

namespace CodeCaster.SerializeThis
{
    public class DebugValueParser : SymbolParser<Expression>
    {
        protected override TypeEnum? GetKnownValueType(Expression typeSymbol)
        {
            var value = typeSymbol.Value;

            var valueTypeName = typeSymbol.Name.EndsWith("?")
                ? typeSymbol.Name.Substring(0, typeSymbol.Name.Length - 1)
                : typeSymbol.Name;

            switch (valueTypeName)
            {
                case "int":
                case "System.Int32":
                    return TypeEnum.Int32;

            }

            return TypeEnum.String;
        }

        protected override ClassInfo GetArrayTypeParameter(Expression typeSymbol)
        {
            throw new NotImplementedException();
        }

        protected override IList<AttributeInfo> GetAttributes(Expression typeSymbol)
        {
            throw new NotImplementedException();
        }

        protected override string GetCacheName(Expression typeSymbol)
        {
            throw new NotImplementedException();
        }

        protected override IList<ClassInfo> GetChildProperties(Expression typeSymbol, object value)
        {
            throw new NotImplementedException();
        }

        protected override string GetClassName(Expression typeSymbol)
        {
            throw new NotImplementedException();
        }

        protected override ClassInfo GetCollectionTypeParameter(Expression typeSymbol)
        {
            throw new NotImplementedException();
        }

        protected override TypeEnum GetComplexSymbolType(Expression typeSymbol, out CollectionType? collectionType, out bool isNullableValueType, ref bool isEnum, out List<ClassInfo> typeParameters)
        {
            throw new NotImplementedException();
        }

        protected override (ClassInfo TKey, ClassInfo TValue) GetDictionaryKeyType(Expression typeSymbol)
        {
            throw new NotImplementedException();
        }

        protected override bool IsEnum(Expression typeSymbol)
        {
            throw new NotImplementedException();
        }
    }
}