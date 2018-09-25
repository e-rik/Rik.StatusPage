using System;
using System.Linq;

namespace Rik.StatusPage.Internal
{
    public static class TypeHelper
    {
        public static Type FindType(string qualifiedName)
        {
            var type = Type.GetType(qualifiedName);
            if (type != null)
                return type;

            var nameParts = qualifiedName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length < 2)
                return null;

            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == nameParts[1].Trim())?
                .GetType(nameParts[0].Trim());
        }

        public static Type FindTypeOrFailWith(string qualifiedName, string message)
        {
            return FindType(qualifiedName) ?? throw new Exception(message);
        }
    }
}