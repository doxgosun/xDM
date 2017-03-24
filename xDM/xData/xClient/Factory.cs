using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace xDM.xData.xClient
{
    public static class Factory
    {

        #region 方法 此类仅仅仅能实例化本命名空间的类
        /// <summary>
        /// 数据库类型到命名空间的缓存
        /// </summary>
        private static ConcurrentDictionary<string, string> _dicDbTypeToNameSpaces = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 命名空间到数据库类型的缓存
        /// </summary>
        private static ConcurrentDictionary<string, string> _dicNameSpacesToDbType = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 命名空间到程序集合的缓存
        /// </summary>
        private static ConcurrentDictionary<string, Assembly> _dicNameSpaceToAssemblys = new ConcurrentDictionary<string, Assembly>();
        /// <summary>
        /// 数据库类型+类名到其对应类型的缓存
        /// </summary>
        private static ConcurrentDictionary<string, Type> _dicDbTypeRegToType = new ConcurrentDictionary<string, Type>();


        public static Assembly GetAssembly(ClientType type)
        {
            return GetAssembly(type + "");
        }
        public static Assembly GetAssembly(string dbType)
        {
            Assembly assembly = null;
            if (_dicNameSpaceToAssemblys.TryGetValue(dbType, out assembly))
                return assembly;
            Assembly myAssembly = Assembly.GetCallingAssembly();
            var basePath = Path.GetDirectoryName(myAssembly?.CodeBase?.Replace("file:///", ""));
            var dChar = Path.DirectorySeparatorChar;
            if (dChar == '/') basePath = $"/{basePath}";
            basePath = $"{basePath}{dChar}xDbDrivers";
            if(!Directory.Exists(basePath))
                basePath = $"{Environment.GetEnvironmentVariable("windir")}{dChar}xDbDrivers";
            string nameSpace = "", dllFile = "";
            if (IntPtr.Size == 4)
                basePath = $@"{basePath}{dChar}x86";
            else
                basePath = $@"{basePath}{dChar}x64";
            var di = new DirectoryInfo($@"{basePath}{dChar}{dbType}");
            var ds = di.GetDirectories();
            foreach (var d in ds)
            {
                nameSpace = d.Name;
                di = new DirectoryInfo($@"{d.FullName}");
                var dllDirs = di.GetDirectories();
                foreach (var dllDi in dllDirs)
                    dllFile = dllDi.Name;
            }
            if (nameSpace == "System.Data.SqlClient")
                assembly = Assembly.GetAssembly(typeof(SqlCommand));
            else if (nameSpace == "System.Data.OleDb")
                assembly = Assembly.GetAssembly(typeof(OleDbCommand));
            else if (nameSpace == "System.Data.Odbc")
                assembly = Assembly.GetAssembly(typeof(OdbcCommand));
            else
                assembly = Assembly.LoadFrom($@"{basePath}{dChar}{dbType}{dChar}{nameSpace}{dChar}{dllFile}{dChar}{dllFile}.dll");
            if (assembly == null)
                throw new Exception("加载程序集失败！");
            _dicDbTypeToNameSpaces.TryAdd(dbType, nameSpace);
            _dicNameSpacesToDbType.TryAdd(nameSpace, dbType);
            _dicNameSpaceToAssemblys.TryAdd(dbType, assembly);
            return assembly;
        }
        public static string GetDbType(string NameSpace)
        {
            string dbType = "";
            _dicDbTypeToNameSpaces.TryGetValue(NameSpace + "", out dbType);
            return dbType;
        }
        public static T GetInstance<T>(string dbType, Regex regInstanceName) where T:class
        {
            return GetInstance<T>(dbType, regInstanceName, null);
        }
        public static T GetInstance<T>(string dbType, Regex regInstanceName, object[] args, Type[] argTypes) where T:class
        {
            Type oType;
            if (_dicDbTypeRegToType.TryGetValue($"{dbType}{regInstanceName}", out oType))
                return GetInstance<T>(oType, args, argTypes);
            var assembly = GetAssembly(dbType);
            var t = typeof(T); object obj = null;
            string nameSpace;
            if (!_dicDbTypeToNameSpaces.TryGetValue(dbType, out nameSpace))
                return null;
            var moudles = assembly.GetModules();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.Namespace == nameSpace && t.IsAssignableFrom(type) && regInstanceName.IsMatch(type.Name))
                {
                    obj = GetInstance<T>(type, args, argTypes);
                    // obj = Activator.CreateInstance(type, args);
                    if (obj != null)
                    {
                        _dicDbTypeRegToType.TryAdd($"{dbType}{regInstanceName}", type);
                        break;
                    }
                }
            }
            return (T)obj;
        }
        public static T GetInstance<T>(string dbType, Regex regInstanceName, object[] args) where T:class
        {
            return GetInstance<T>(dbType, regInstanceName, args, null);
        }
        public static T GetInstance<T>(Type type, object[] args, Type[] argTypes)
        {
            var del = _GetEmitCreateFunc(type, args, argTypes);
            object obj;
            if (args == null)
                obj = ((Func<object>)del)();
            else
                obj = del.DynamicInvoke(args);
            return (T)obj;
        }

        private static Delegate _GetEmitCreateFunc(Type type, object[] args, Type[] argTypes)
        {
            Delegate fun;
            if (argTypes != null)
                fun = DelegateBuilder.GetCreateDelegate(type, argTypes);
            else
                fun = DelegateBuilder.GetCreateDelegate(type, DelegateBuilder.GetParamsTypesFromParameters(args));
            return fun;
        }


		/// <summary>
		/// Gets the namespace.
		/// </summary>
		/// <returns>The namespace.</returns>
		/// <param name="dbType">Db type.</param>
        public static string GetNamespace(string dbType)
        {
            string nameSpace = "";
            _dicDbTypeToNameSpaces.TryGetValue(dbType + "", out nameSpace);
            return nameSpace;
        }


		/// <summary>
		/// Creates the command.
		/// </summary>
		/// <returns>The command.</returns>
		/// <param name="dbType">Db type.</param>
        public static IDbCommand CreateCommand(ClientType dbType)
        {
            return CreateCommand(dbType + "");
        }
        static Regex regCommand = new Regex(".*Command$");
        public static IDbCommand CreateCommand(string dbType)
        {
            return GetInstance<IDbCommand>(dbType, regCommand);
        }
		/// <summary>
		/// Creates the connection.
		/// </summary>
		/// <returns>The connection.</returns>
		/// <param name="dbType">Db type.</param>
        public static IDbConnection CreateConnection(ClientType dbType)
        {
            return CreateConnection(dbType + "");
        }
        static Regex regConnection = new Regex(".*Connection$");
        public static IDbConnection CreateConnection(string dbType)
        {
            return GetInstance<IDbConnection>(dbType, regConnection);
        }
		/// <summary>
		/// Creates the data adapter.
		/// </summary>
		/// <returns>The data adapter.</returns>
		/// <param name="dbType">Db type.</param>
        public static IDbDataAdapter CreateDataAdapter(ClientType dbType)
        {
            return CreateDataAdapter(dbType + "");
        }
        static Regex regDataAdapte = new Regex(".*DataAdapter$");
        public static IDbDataAdapter CreateDataAdapter(string dbType)
        {
            return GetInstance<IDbDataAdapter>(dbType, regDataAdapte);
        }

        static Regex regParameter = new Regex(".*Parameter$");
        public static IDbDataParameter CreateDataParameter(IDataParameterCollection dataParameterCollection)
        {
            var nameSpace = dataParameterCollection.GetType().Namespace;
            string strType = ClientType.Unknow.ToString();
            _dicNameSpacesToDbType.TryGetValue(nameSpace, out strType);
            return GetInstance<IDbDataParameter>(strType, regParameter);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="dbType">数据库类型</param>
        /// <param name="className">类名</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static TResult CreateOther<TResult>(string dbType, string className, object[] args) where TResult : class
        {
            return GetInstance<TResult>(dbType, new Regex(className.Replace(".", "\\.")), args);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="dbType">数据库类型</param>
        /// <param name="className">类名</param>
        /// <param name="args">参数</param>
        /// <param name="argTypes">构造函数原始参数类型</param>
        /// <returns></returns>
        public static TResult CreateOther<TResult>(string dbType, string className, object[] args, Type[] argTypes) where TResult : class
        {
            return GetInstance<TResult>(dbType, new Regex(className.Replace(".", "\\.")), args, argTypes);
        }


        #endregion
    }
}
