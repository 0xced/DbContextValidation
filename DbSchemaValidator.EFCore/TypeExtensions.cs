using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DbSchemaValidator.EFCore
{
    internal static class TypeExtensions
    {
        private class SimpleTypeComparer : IEqualityComparer<Type>
        {
            public static SimpleTypeComparer Instance => new SimpleTypeComparer();
            
            public bool Equals(Type a, Type b)
            {
                if (a == null)
                    throw new ArgumentNullException(nameof(a));
                if (b == null)
                    throw new ArgumentNullException(nameof(b));
                
                return a.Assembly == b.Assembly &&
                       a.Namespace == b.Namespace &&
                       a.Name == b.Name;
            }
            
            public int GetHashCode(Type obj)
            {
                return $"{obj.Assembly}.{obj.Namespace}.{obj.Name}".GetHashCode();
            }
        }
        
        internal static MethodInfo GetMethod(this Type type, Type returnType, string name, params Type[] parameterTypes)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (returnType == null)
                throw new ArgumentNullException(nameof(returnType));
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (parameterTypes == null)
                throw new ArgumentNullException(nameof(parameterTypes));
            
            foreach (var method in type.GetMethods().Where(m => m.Name == name))
            {
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType);
                var returnTypeMatch = SimpleTypeComparer.Instance.Equals(returnType, method.ReturnType);
                var parameterTypesMatch = methodParameterTypes.SequenceEqual(parameterTypes, SimpleTypeComparer.Instance);
                if (returnTypeMatch && parameterTypesMatch)
                {
                    return method;
                }
            }
            throw new MissingMethodException($"Method {name}({string.Join(", ", parameterTypes.Select(p => p.Name))}) was not found.");
        }
    }
}
