using System;
using System.Reflection;

namespace xDM.xReflection
{
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