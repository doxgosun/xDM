using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xReflection
{
    public static partial class DelegateBuilder
    {
        public static Delegate GetMethodDelegate(MethodInfo method)
        {
            if (method == null) return null;
            Delegate retDelegate;
            var key = new KeyValuePair<Type, string>(method.ReflectedType,method.ToString());
            if (_dicStaticTypeToDelegate.TryGetValue(key, out retDelegate))
                return retDelegate;
            retDelegate = _CreateEmitMethodDelegate(method);
            if(retDelegate != null)
                _dicStaticTypeToDelegate.TryAdd(key, retDelegate);
            return retDelegate;
        }

        public static TDelegate GetMethodDelegate<TDelegate>(MethodInfo method) where TDelegate : class
        {
            return GetMethodDelegate(method) as TDelegate;
        }

        private static Delegate _CreateEmitMethodDelegate(MethodInfo method)
        {
            if (method == null) return null;
            if (method.IsStatic)
                return method.CreateDelegate(_GetDelegateType(method));
            var parameters = method.GetParameters();
            var len = parameters.Length;
            var methodParaTypes = new Type[len];
            var paraTypes = new Type[len + 1];
            paraTypes[0] = method.ReflectedType;
            for (int i = 0; i < len; i++)
                paraTypes[i + 1] = methodParaTypes[i] = parameters[i].ParameterType;
            DynamicMethod emitMethod = new DynamicMethod(string.Empty, method.ReturnType, paraTypes, method.ReflectedType.Module);
            var il = emitMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            Type pType;
            LocalBuilder local;
            for (int i = 1; i < len + 1; i++)
            {
                pType = paraTypes[i];
                local = il.DeclareLocal(pType, true);
                il.Emit(OpCodes.Ldarg, i);
                if (pType.IsValueType)
                    il.Emit(OpCodes.Unbox_Any, pType);
                else
                    il.Emit(OpCodes.Castclass, pType);
                il.Emit(OpCodes.Stloc, local);
                il.Emit(OpCodes.Ldloc, local);
            }
            il.EmitCall(OpCodes.Callvirt, method, null);
            il.Emit(OpCodes.Ret);
            var delType = _GetDelegateType(paraTypes, method.ReturnType);
            return emitMethod.CreateDelegate(delType);
        }

    }
}
