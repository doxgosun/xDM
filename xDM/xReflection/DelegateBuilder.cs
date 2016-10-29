using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Reflection;

namespace xDM.xReflection
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
            else if(ctors.Length > 0)
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

    }
}
