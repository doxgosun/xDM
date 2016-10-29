using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace xDM.xReflection
{
    partial class DelegateBuilder
    {
        public static TDelegate GetGetterDelegate<TDelegate>(FieldInfo field) where TDelegate:class
        {
            return GetGetterDelegate(field) as TDelegate;
        }
        public static Delegate GetGetterDelegate(FieldInfo field)
        {
            return _GetEmitFieldDelegate(field, false);
        }
        public static TDelegate GetSetterDelegate<TDelegate>(FieldInfo field) where TDelegate : class
        {
            return GetSetterDelegate(field) as TDelegate;
        }
        public static Delegate GetSetterDelegate(FieldInfo field)
        {
            return _GetEmitFieldDelegate(field, true);
        }

        private static Delegate _GetEmitFieldDelegate(FieldInfo field, bool isSetter)
        {
            if (field == null) return null;
            Delegate retDelegate;
            var key = new KeyValuePair<Type, string>(field.ReflectedType, field + isSetter.ToString());
            if (_dicStaticTypeToDelegate.TryGetValue(key, out retDelegate))
                return retDelegate;

            var paraTypesList = new List<Type>();
            if(!field.IsStatic)
                paraTypesList.Add(field.ReflectedType);
            DynamicMethod emitMethod;
            if (isSetter)
            {
                paraTypesList.Add(field.FieldType);
                emitMethod = new DynamicMethod(string.Empty, null, paraTypesList.ToArray(), field.ReflectedType.Module);
            }
            else
                emitMethod = new DynamicMethod(string.Empty, field.FieldType, paraTypesList.ToArray(), field.ReflectedType.Module);
            var il = emitMethod.GetILGenerator();
            for (int i = 0; i < paraTypesList.Count; i++)
                il.Emit(OpCodes.Ldarg, i);
            Type delType;
            if (isSetter)
            {
                il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
                delType = _GetDelegateType(paraTypesList.ToArray(), null);
            }
            else
            {
                il.Emit(field.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, field);
                delType = _GetDelegateType(paraTypesList.ToArray(), field.FieldType);
            }
            il.Emit(OpCodes.Ret);

            retDelegate = emitMethod.CreateDelegate(delType);
            if (retDelegate != null)
                _dicStaticTypeToDelegate.TryAdd(key, retDelegate);
            return retDelegate;
        }
    }
}
