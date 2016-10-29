/*
 这里提供反射的委托创建，用于提高重复用反射创建实例等的效率
 Make By GoSun 20161026 
 创建器自带缓存，默认开启，可设置关闭   SetCache(false)，关闭缓存后会清空缓存
 !!!!只缓存构造函数和静态方法、属性、字段的委托，实体类相关委托不缓存！！！
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Collections.Concurrent;

namespace xDM.xData.xClient
{
    public static partial class DelegateBuilder
    {
        #region 缓存
        /// <summary>
        /// 构造函数类型到构造函数委托的缓存 Key:KV(返回类型Type,构造函数.ToString) Value:委托
        /// </summary>
        private static ConcurrentDictionary<KeyValuePair<Type, string>, Delegate> _dicFunTypeToFunc { get; set; } = new ConcurrentDictionary<KeyValuePair<Type, string>, Delegate>();
        /// <summary>
        /// 委托的(parameterTypes.Name,ReturnType.Name)到委托Type的缓存
        /// </summary>
        private static ConcurrentDictionary<string, Type> _dicDelStringToDelType { get; set; } = new ConcurrentDictionary<string, Type>();
        /// <summary>
        /// 方法、属性、字段的缓存 Key:KV(原对象类型Type,方法.ToString) Value:委托
        /// </summary>
        private static ConcurrentDictionary<KeyValuePair<Type, string>, Delegate> _dicStaticTypeToDelegate { get; set; } = new ConcurrentDictionary<KeyValuePair<Type, string>, Delegate>();
        private static bool _isCache = true;
        #endregion

        #region 一些方法
        /// <summary>
        /// 设置开启或关闭缓存
        /// </summary>
        /// <param name="isCache"></param>
        public static void SetCache(bool isCache)
        {
            if (!isCache)
            {
                _dicFunTypeToFunc = null;
                _dicDelStringToDelType = null;
                _dicStaticTypeToDelegate = null;
            }
            else
            {
                _dicFunTypeToFunc = new ConcurrentDictionary<KeyValuePair<Type, string>, Delegate>();
                _dicDelStringToDelType = new ConcurrentDictionary<string, Type>();
                _dicStaticTypeToDelegate = new ConcurrentDictionary<KeyValuePair<Type, string>, Delegate>();
            }
            _isCache = isCache;
        }
        /// <summary>
        /// 是否缓存
        /// </summary>
        /// <returns></returns>
        public static bool IsCache() { return _isCache; }
        private static string _DelSignToString(Type[] argsTypes, Type retType)
        {
            var delSign = new StringBuilder();
            if (argsTypes == null)
                delSign.Append(",");
            else
                foreach (var t in argsTypes)
                    delSign.Append(t?.FullName);
            delSign.Append(",");
            delSign.Append(retType?.FullName);
            return delSign.ToString();
        }

        #endregion


        /// <summary>
        /// 通过参数获取参数类型
        /// </summary>
        /// <param name="oParams"></param>
        /// <returns></returns>
        public static Type[] GetParamsTypesFromParameters(object[] oParams)
        {
            Type[] argTypes = null;
            if (oParams != null)
            {
                argTypes = new Type[oParams.Length];
                for (int i = 0; i < oParams.Length; i++)
                    argTypes[i] = oParams[i] == null ? null : oParams[i].GetType();
            }
            return argTypes;
        }
        /// <summary>
        /// 通过参数获取参数类型
        /// </summary>
        /// <param name="oParams"></param>
        /// <returns></returns>
        public static Type[] GetParamsTypesFromParameters(ParameterInfo[] oParams)
        {
            Type[] argTypes = null;
            if (oParams != null)
            {
                argTypes = new Type[oParams.Length];
                for (int i = 0; i < oParams.Length; i++)
                    argTypes[i] = oParams[i] == null ? null : oParams[i].ParameterType;
            }
            return argTypes;
        }

        /// <summary>
        /// 通过已知参数类型获取相应构造函数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="paramTypes"></param>
        /// <returns></returns>
        public static ConstructorInfo GetConstructorInfo(Type type, Type[] paramTypes)
        {
            if (type == null) return null;
            if (paramTypes == null) paramTypes = new Type[0];
            ConstructorInfo ctor = type.GetConstructor(paramTypes);
            if (ctor != null) return ctor;
            var ctors = type.GetConstructors();
            if (paramTypes.Length > 0)
            {
                ParameterInfo[] paramInfos;
                ParameterInfo pi; Type t;
                var isCtor = false;
                foreach (var ct in ctors)
                {
                    if (ctor != null)
                        break;
                    paramInfos = ct.GetParameters();
                    if (paramInfos.Length != paramTypes.Length)
                        continue;
                    isCtor = true;
                    for (int i = 0; i < paramInfos.Length; i++)
                    {
                        pi = paramInfos[i];
                        t = paramTypes[i];
                        if (t != null && t != Type.Missing && pi.ParameterType != t)
                        {
                            isCtor = false;
                            break;
                        }
                    }
                    if (isCtor)
                        ctor = ct;
                }
            }
            else if (ctors.Length > 0)
                ctor = type.GetConstructors()[0];
            return ctor;
        }

        #region 获取泛型委托的类型

        private static Type _GetDelegateType(Type[] paraTypes, Type retType)
        {
            if ((paraTypes == null || paraTypes.Length == 0) && retType == null)
                return null;
            Type delTpye = null;
            string key = null;
            if (_dicDelStringToDelType != null)
            {
                key = _DelSignToString(paraTypes, retType);
                if (_dicDelStringToDelType.TryGetValue(key, out delTpye))
                    return delTpye;
            }
            if (retType != null && retType != typeof(void) && retType != Type.Missing)
                delTpye = DelegateType.GetFuncType(paraTypes, retType);
            else
                delTpye = DelegateType.GetActionType(paraTypes);
            if (_dicDelStringToDelType != null && key != null)
                _dicDelStringToDelType.TryAdd(key, delTpye);
            return delTpye;
        }
        private static Type _GetDelegateType(MethodInfo methnod)
        {
            if (methnod == null) return null;
            var parameters = methnod.GetParameters();
            var len = parameters.Length;
            var paraTypes = new Type[len];
            for (int i = 0; i < len; i++)
                paraTypes[i] = parameters[i].ParameterType;
            return _GetDelegateType(paraTypes, methnod.ReturnType);
        }
        #endregion
        public static TFunc GetCreateDelegate<TFunc>(Type type) where TFunc : class
        {
            return GetCreateDelegate(GetConstructorInfo(type, null)) as TFunc;
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
        public static Delegate GetCreateDelegate(Type type, Type[] paramTypes)
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
        private static Delegate _GetEmitCreateDelegate(ConstructorInfo ctor)
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
            DynamicMethod dlgMethod = new DynamicMethod(String.Empty, ctor.ReflectedType, paramTypes, ctor.Module);
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
        private static Delegate _GetExpressionCreateDelegate(ConstructorInfo ctor)
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
                _dicFunTypeToFunc.TryGetValue(new KeyValuePair<Type, string>(ctor.ReflectedType, ctor.ToString()), out func);
            return func;
        }
        private static void _SetCreateDelegateToCache(ConstructorInfo ctor, Delegate delFunc)
        {
            if (_dicFunTypeToFunc != null && ctor != null && delFunc != null)
                _dicFunTypeToFunc.TryAdd(new KeyValuePair<Type, string>(ctor.ReflectedType, ctor.ToString()), delFunc);
        }


    }
    public class DelegateType
    {
        public static Type GetFuncType(Type[] paraTypes, Type retType)
        {
            var paraCount = 0;
            Type[] types = null;
            if (paraTypes != null)
            {
                paraCount = paraTypes.Length;
                types = new Type[paraCount + 1];
                paraTypes.CopyTo(types, 0);
            }
            else
                types = new Type[1];
            types[types.Length - 1] = retType;
            MethodInfo method = typeof(DelegateType).GetMethod($"{nameof(GetFuncType)}{paraCount}")?.MakeGenericMethod(types);
            return method?.Invoke(null, null) as Type;
        }
        public static Type GetActionType(Type[] paraTypes)
        {
            MethodInfo method = typeof(DelegateType).GetMethod($"{nameof(GetActionType)}{paraTypes.Length}")?.MakeGenericMethod(paraTypes);
            return method?.Invoke(null, null) as Type;
        }
        public static Type GetPredicateType(Type[] paraTypes)
        {
            MethodInfo method = typeof(DelegateType).GetMethod($"{nameof(GetPredicateType)}{paraTypes.Length}")?.MakeGenericMethod(paraTypes);
            return method?.Invoke(null, null) as Type;
        }

        #region 16 Func<>()
        public static Type GetFuncType16<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>()
        {
            return typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>);
        }
        public static Type GetFuncType15<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>()
        {
            return typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>);
        }
        public static Type GetFuncType14<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>()
        {
            return typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>);
        }
        public static Type GetFuncType13<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>()
        {
            return typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>);
        }
        public static Type GetFuncType12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>()
        {
            return typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>);
        }
        public static Type GetFuncType11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>()
        {
            return typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>);
        }
        public static Type GetFuncType10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>()
        {
            return typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>);
        }
        public static Type GetFuncType9<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>()
        {
            return typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>);
        }
        public static Type GetFuncType8<T1, T2, T3, T4, T5, T6, T7, T8, TResult>()
        {
            return typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>);
        }
        public static Type GetFuncType7<T1, T2, T3, T4, T5, T6, T7, TResult>()
        {
            return typeof(Func<T1, T2, T3, T4, T5, T6, T7, TResult>);
        }
        public static Type GetFuncType6<T1, T2, T3, T4, T5, T6, TResult>()
        {
            return typeof(Func<T1, T2, T3, T4, T5, T6, TResult>);
        }
        public static Type GetFuncType5<T1, T2, T3, T4, T5, TResult>()
        {
            return typeof(Func<T1, T2, T3, T4, T5, TResult>);
        }
        public static Type GetFuncType4<T1, T2, T3, T4, TResult>()
        {
            return typeof(Func<T1, T2, T3, T4, TResult>);
        }
        public static Type GetFuncType3<T1, T2, T3, TResult>()
        {
            return typeof(Func<T1, T2, T3, TResult>);
        }
        public static Type GetFuncType2<T1, T2, TResult>()
        {
            return typeof(Func<T1, T2, TResult>);
        }
        public static Type GetFuncType1<T1, TResult>()
        {
            return typeof(Func<T1, TResult>);
        }
        public static Type GetFuncType0<TResult>()
        {
            return typeof(Func<TResult>);
        }
        #endregion

        #region 16 Action<>()
        public static Type GetActionType16<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>()
        {
            return typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>);
        }
        public static Type GetActionType15<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>()
        {
            return typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>);
        }
        public static Type GetActionType14<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>()
        {
            return typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>);
        }
        public static Type GetActionType13<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>()
        {
            return typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>);
        }
        public static Type GetActionType12<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>()
        {
            return typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>);
        }
        public static Type GetActionType11<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>()
        {
            return typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>);
        }
        public static Type GetActionType10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
        {
            return typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>);
        }
        public static Type GetActionType9<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
        {
            return typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>);
        }
        public static Type GetActionType8<T1, T2, T3, T4, T5, T6, T7, T8>()
        {
            return typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8>);
        }
        public static Type GetActionType7<T1, T2, T3, T4, T5, T6, T7>()
        {
            return typeof(Action<T1, T2, T3, T4, T5, T6, T7>);
        }
        public static Type GetActionType6<T1, T2, T3, T4, T5, T6>()
        {
            return typeof(Action<T1, T2, T3, T4, T5, T6>);
        }
        public static Type GetActionType5<T1, T2, T3, T4, T5>()
        {
            return typeof(Action<T1, T2, T3, T4, T5>);
        }
        public static Type GetActionType4<T1, T2, T3, T4>()
        {
            return typeof(Action<T1, T2, T3, T4>);
        }
        public static Type GetActionType3<T1, T2, T3>()
        {
            return typeof(Action<T1, T2, T3>);
        }
        public static Type GetActionType2<T1, T2>()
        {
            return typeof(Action<T1, T2>);
        }
        public static Type GetActionType1<T1>()
        {
            return typeof(Action<T1>);
        }
        public static Type GetActionType0()
        {
            return typeof(Action);
        }
        #endregion

        #region 1 Predicate
        public static Type GetPredicate<T>()
        {
            return typeof(Predicate<T>);
        }
        #endregion

    }

}
