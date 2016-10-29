using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xReflection
{
    static partial class DelegateBuilder
    {
        public static TFunc GetCreateDelegate<TFunc>(this Type type) where TFunc : class
        {
            return GetCreateDelegate(GetConstructorInfo(type,null)) as TFunc;
        }
        /// <summary>
        /// 获取构造函数的委托
        /// </summary>
        /// <typeparam name="TFunc"></typeparam>
        /// <param name="type"></param>
        /// <param name="paramTypes"></param>
        /// <returns></returns>
        public static TFunc GetCreateDelegate<TFunc>(Type type, Type[] paramTypes) where TFunc : class
        {
            return GetCreateDelegate(GetConstructorInfo(type, paramTypes)) as TFunc;
        }

        public static TFunc GetCreateDelegate<TFunc>(ConstructorInfo ctor) where TFunc : class
        {
            return GetCreateDelegate(ctor) as TFunc;
        }
        public static Delegate GetCreateDelegate(Type type)
        {
            return GetCreateDelegate(GetConstructorInfo(type, null));
        }
        /// <summary>
        /// 获取构造函数的委托
        /// </summary>
        /// <param name="type"></param>
        /// <param name="paramTypes"></param>
        /// <returns></returns>
        public static Delegate GetCreateDelegate(this Type type, Type[] paramTypes)
        {
            return GetCreateDelegate(GetConstructorInfo(type, paramTypes));
        }
        public static Delegate GetCreateDelegate(ConstructorInfo ctor)
        {
            Delegate retDele = _GetCreateDelegateFromCache(ctor);
            if (retDele != null)
                return retDele;
            retDele = _GetEmitCreateDelegate(ctor);
            if (retDele != null)
                _SetCreateDelegateToCache(ctor, retDele);
            return retDele;
        }

        /// <summary>
        /// 通过构造函数返回类型，构造函数，委托类型获取构造函数的委托
        /// </summary>
        /// <param name="retType">构造函数返回类型</param>
        /// <param name="ctor">构造函数</param>
        /// <param name="delType">返回指定委托类型</param>
        /// <returns></returns>
        private static Delegate _GetEmitCreateDelegate(this ConstructorInfo ctor)
        {
            if (ctor == null) return null;
            Type[] paramTypes = null;
            var paramInfos = ctor.GetParameters();
            var len = paramInfos.Length;
            if (len > 0)
            {
                paramTypes = new Type[len];
                for (int i = 0; i < len; i++)
                    paramTypes[i] = paramInfos[i].ParameterType;
            }
            DynamicMethod dlgMethod = new DynamicMethod(String.Empty, ctor.ReflectedType, paramTypes,ctor.Module);
            ILGenerator il = dlgMethod.GetILGenerator();
            for (int i = 0; i < len; i++)
                il.Emit(OpCodes.Ldarg, i);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ret);
            var delType = _GetDelegateType(paramTypes, ctor.ReflectedType);
            return dlgMethod.CreateDelegate(delType);
        }
        /// <summary>
        /// 获取Expression方式构建的构造函数委托
        /// </summary>
        /// <param name="type"></param>
        /// <param name="paramTypes"></param>
        /// <returns></returns>
        private static Delegate _GetExpressionCreateDelegate(this ConstructorInfo ctor)
        {
            var newExpression = Expression.New(ctor.ReflectedType);
            var paramInfos = ctor.GetParameters();
            ParameterExpression[] parameters = null;
            var len = paramInfos.Length;
            Type[] paraTypes = null;
            if (len > 0)
            {
                parameters = new ParameterExpression[len];
                paraTypes = new Type[len];
                for (int i = 0; i < len; i++)
                {
                    parameters[i] = Expression.Parameter(paramInfos[i].ParameterType);
                    paraTypes[i] = paramInfos[i].ParameterType;
                }
            }
            var delType = _GetDelegateType(paraTypes, ctor.ReflectedType);
            return Expression.Lambda(delType, newExpression, parameters).Compile();
        }

        private static Delegate _GetCreateDelegateFromCache(ConstructorInfo ctor)
        {
            Delegate func = null;
            if (_dicFunTypeToFunc != null && ctor != null)
                _dicFunTypeToFunc.TryGetValue(new KeyValuePair<Type, string>(ctor.ReflectedType,ctor.ToString()), out func);
            return func;
        }
        private static void _SetCreateDelegateToCache(ConstructorInfo ctor, Delegate delFunc)
        {
            if (_dicFunTypeToFunc != null && ctor != null && delFunc != null)
                _dicFunTypeToFunc.TryAdd(new KeyValuePair<Type, string>(ctor.ReflectedType,ctor.ToString()), delFunc);
        }
    }
}
