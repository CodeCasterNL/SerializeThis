using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace CodeCaster.SerializeThis.Serialization.Roslyn
{
    public class TypeSymbolParser
    {
        public Class GetMemberInfoRecursive(ITypeSymbol typeSymbol, SemanticModel semanticModel)
        {
            Class memberInfo = GetMemberInfoRecursive(typeSymbol.Name, typeSymbol);

            return memberInfo;
        }

        private Class GetMemberInfoRecursive(string name, ITypeSymbol typeSymbol)
        {
            bool isCollection;
            bool isNullable;
            var type = GetSymbolType(typeSymbol, out isCollection, out isNullable);

            var thisClass = new Class
            {
                Name = name,
                Type = type,
                IsCollection = isCollection,
                IsNullable = isNullable,
            };

            if (thisClass.IsCollection || thisClass.Type == TypeEnum.ComplexType && !isNullable)
            {
                // A collection's first child is its collection type.
                thisClass.Children = GetChildProperties(typeSymbol);
            }

            return thisClass;
        }

        private IList<Class> GetChildProperties(ITypeSymbol typeSymbol)
        {
            var result = new List<Class>();

            if (typeSymbol.BaseType != null)
            {
                result.AddRange(GetChildProperties(typeSymbol.BaseType));
            }

            foreach (var member in typeSymbol.GetMembers())
            {
                if (member.Kind == SymbolKind.Property && member.DeclaredAccessibility == Accessibility.Public)
                {
                    var memberTypeSymbol = member as IPropertySymbol;
                    if (memberTypeSymbol != null)
                    {
                        result.Add(GetMemberInfoRecursive(memberTypeSymbol.Name, memberTypeSymbol.Type));
                    }
                }
            }

            return result;
        }

        private TypeEnum GetSymbolType(ITypeSymbol typeSymbol, out bool isCollection, out bool isNullable)
        {
            isNullable = IsNullableType(typeSymbol);
            isCollection = IsCollectionType(ref typeSymbol);

            if (isNullable)
            {
                var named = typeSymbol as INamedTypeSymbol;
                var nullableType = named?.TypeArguments.FirstOrDefault();
                if (nullableType != null)
                {
                    bool ignored;
                    var nullableTypeEnum = GetSymbolType(nullableType, out ignored, out ignored);
                    return nullableTypeEnum;
                }
            }

            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return TypeEnum.Boolean;
                case SpecialType.System_String:
                    return TypeEnum.String;
                case SpecialType.System_DateTime:
                    return TypeEnum.DateTime;
                case SpecialType.System_Int32:
                    return TypeEnum.Int32;
            }

            return TypeEnum.ComplexType;
        }

        private bool IsNullableType(ITypeSymbol typeSymbol)
        {
            var named = typeSymbol as INamedTypeSymbol;
            if (named == null)
            {
                return false;
            }

            return typeSymbol.TypeKind == TypeKind.Struct && named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;
        }

        private bool IsCollectionType(ref ITypeSymbol typeSymbol)
        {
            // TODO: store known collection collection somewhere with their interface info, so we know which properties to ignore (Item[], Syncroot, Count, AllKeys, ...).
            string[] knownCollectionInterfaces = { "System.IEnumerable", "System.Collections.Generic" };

            return typeSymbol.Interfaces.Any(i => knownCollectionInterfaces.Any(c => c == GetClrName(i.Name)));
        }

        private string GetClrName(string argName)
        {
            return argName;
        }
    }
}
