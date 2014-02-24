using System;
using System.Reflection;
using System.Reflection.Emit;

namespace SharpMap.Data.Providers.Business
{
    public class TypeUtility<TObjectType>
    {
        public delegate TMemberType MemberGetDelegate<out TMemberType>(TObjectType obj);

        internal delegate object MemberGetDelegate(TObjectType obj);

        internal static MemberGetDelegate<TMemberType> GetMemberGetDelegate<TMemberType>(string memberName)
        {
            var objectType = typeof(TObjectType);
            
            var pi = objectType.GetProperty(memberName);
            var fi = objectType.GetField(memberName);
            if (pi != null)
            {
                // Member is a Property...

                var mi = pi.GetGetMethod();
                if (mi != null)
                {
                    // NOTE:  As reader J. Dunlap pointed out...
                    //  Calling a property's get accessor is faster/cleaner using
                    //  Delegate.CreateDelegate rather than Reflection.Emit 
                    return (MemberGetDelegate<TMemberType>)
                        Delegate.CreateDelegate(typeof(MemberGetDelegate<TMemberType>), mi);
                }
                throw new Exception(String.Format(
                    "Property: '{0}' of Type: '{1}' does" +
                    " not have a Public Get accessor",
                    memberName, objectType.Name));
            }

            if (fi != null)
            {
                // Member is a Field...

                var dm = new DynamicMethod("Get" + memberName,
                    typeof(TMemberType), new[] { objectType }, objectType);
                var il = dm.GetILGenerator();
                // Load the instance of the object (argument 0) onto the stack
                il.Emit(OpCodes.Ldarg_0);
                // Load the value of the object's field (fi) onto the stack
                il.Emit(OpCodes.Ldfld, fi);
                // return the value on the top of the stack
                il.Emit(OpCodes.Ret);

                return (MemberGetDelegate<TMemberType>)
                    dm.CreateDelegate(typeof(MemberGetDelegate<TMemberType>));
            }

            throw new Exception(String.Format(
                "Member: '{0}' is not a Public Property or Field of Type: '{1}'",
                memberName, objectType.Name));
        }

        internal static MemberGetDelegate<TMemberType> GetMemberGetDelegate<TMemberType>(Type attributeType )
        {
            var objectType = typeof (TObjectType);
            return GetMemberGetDelegate<TMemberType>(objectType, attributeType);
        }

        internal static MemberGetDelegate<TMemberType> GetMemberGetDelegate<TMemberType>(Type objectType, Type attributeType)
        {
            var pis = objectType.GetProperties(/*BindingFlags.GetProperty | BindingFlags.Public*/);
            foreach (var propertyInfo in pis)
            {
                var att = propertyInfo.GetCustomAttributes(attributeType, true);
                if (att.Length > 0)
                    return GetMemberGetDelegate<TMemberType>(propertyInfo.Name);
            }

            var fis = objectType.GetFields(BindingFlags.GetField | BindingFlags.Public);
            foreach (var fieldInfo in fis)
            {
                var att = fieldInfo.GetCustomAttributes(attributeType, true);
                if (att.Length > 0)
                    return GetMemberGetDelegate<TMemberType>(fieldInfo.Name);
            }

            if (objectType.BaseType != typeof (object))
                return GetMemberGetDelegate<TMemberType>(objectType.BaseType, attributeType);

            throw new ArgumentException("Attribute not declared on public field or property", "attributeType");
        }

        public static Type GetMemberType(string memberName)
        {
            return TypeUtility.GetMemberType(typeof (TObjectType), memberName);
        }
    }

    internal class TypeUtility
    {
        public static Type GetMemberType(Type objectType, string memberName)
        {
            var pi = objectType.GetProperty(memberName);
            var fi = objectType.GetField(memberName);
            if (pi != null)
            {
                // Member is a Property...
                return pi.PropertyType;
            }
            if (fi != null)
            {
                // Member is a Field...
                return fi.FieldType;
            }
            if (objectType.BaseType != typeof (object))
                return GetMemberType(objectType.BaseType, memberName);

            throw new Exception("Member '" + memberName + "' not found in type '"+ objectType.Name + "'!");
        }
    }
}