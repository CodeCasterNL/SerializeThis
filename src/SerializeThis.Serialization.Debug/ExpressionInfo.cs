using EnvDTE;

namespace SerializeThis.Serialization.Debug
{
    public class ExpressionInfo
    {
        public string Path { get; }

        public Expression Expression { get; }

        public TypeInfo Type { get; }

        public ExpressionInfo(string path, Expression expression, TypeInfo type)
        {
            Path = path;
            Expression = expression;
            Type = type;
        }

        public  Expression FindMember(MemberInfo prop)
        {
            foreach (Expression dataMember in Expression.DataMembers)
            {
                if (dataMember.Name == prop.Name)
                {
                    return dataMember;
                }
            }

            return null;
        }
    }
}
