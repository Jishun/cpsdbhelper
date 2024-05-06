using CpsDbHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CpsDbHelper.Support
{
    public delegate object SqlMockCallback(string context, object defaultRet = null, params object[] paramers);
    public class ParentContext
    {
        public string Name { get; private set; }
        public object DefaultRet { get; private set; }
        public object[] DbParams { get; private set; }
        public ParentContext(string name, object defaultRet = null, object[] dbParams = null)
        {
            Name = name;
            DefaultRet = defaultRet;
            DbParams = dbParams;
        }
    }
    public class AdoDotNetDataProviderStub : IAdoNetProviderFactory
    {
        SqlMockCallback Callback;
        public AdoDotNetDataProviderStub(SqlMockCallback callback)
        {
            Callback = callback;
        }
        public IDbConnection CreateConnection(string connectionString)
        {
            return new SqlConnectionStub(Callback);
        }

        public IDbDataParameter CreateParameter(string dataType = null)
        {
            return new DbDataParameterStub() {  DataType = dataType};
        }
    }

    public class SqlConnectionStub : IDbConnection
    {
        SqlMockCallback Callback;
        public SqlConnectionStub(SqlMockCallback callback)
        {
            Callback = callback;
        }

        public string ConnectionString { get; set; }

        public int ConnectionTimeout { get; set; }

        public string Database { get; set; }

        public ConnectionState State { get; set; }

        public IDbTransaction BeginTransaction()
        {
            Callback("SqlConnectionStub::BeginTransaction");
            return new DbTransactionStub(Callback);
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            Callback($"SqlConnectionStub::BeginTransaction::IsolationLevel", null, il);
            return new DbTransactionStub(Callback);
        }

        public void ChangeDatabase(string databaseName)
        {
            Callback($"SqlConnectionStub::ChangeDatabase", null, databaseName);
        }

        public void Close()
        {
            Callback("SqlConnectionStub::Close");
        }

        public IDbCommand CreateCommand()
        {
            Callback("SqlConnectionStub::CreateCommand");
            return new DbCommandStub(Callback);
        }

        public void Dispose()
        {
            Callback("SqlConnectionStub::Dispose");
        }

        public void Open()
        {
            Callback("SqlConnectionStub::Open");
        }
    }

    public class DbCommandStub: IDbCommand
    {
        IDataParameterCollection parameters;
        SqlMockCallback Callback;
        public DbCommandStub(SqlMockCallback callback)
        {
            Callback = callback;
            parameters = new DataParameterCollectionStub(callback, () => CommandText);
        }
        public IDataParameterCollection Parameters => parameters;
        public string CommandText { get; set; }
        public int CommandTimeout { get; set; }
        public CommandType CommandType { get; set; }
        public IDbConnection Connection { get; set; }
        public IDbTransaction Transaction { get; set; }
        public UpdateRowSource UpdatedRowSource { get; set; }

        public void Cancel()
        {
            Callback("DbCommandStub::Cancel");
        }

        public IDbDataParameter CreateParameter()
        {
            Callback("DbCommandStub::CreateParameter");
            return new DbDataParameterStub();
        }

        public void Dispose()
        {
            Callback("DbCommandStub::Dispose");
        }

        public IDataReader ExecuteReader()
        {
            var ps = ToIEnumerable(Parameters.GetEnumerator()).ToArray();
            Callback("DbCommandStub::ExecuteReader(IDataReader)::CommandText", new ParentContext(CommandText), ps);
            return new DataReaderStub(Callback, CommandText, ps);
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            var ps = ToIEnumerable(Parameters.GetEnumerator()).ToArray();
            Callback($"DbCommandStub::ExecuteReader::CommandBehavior(IDataReader)::CommandText", new ParentContext(CommandText), behavior, ps);
            return new DataReaderStub(Callback, CommandText, ps);
        }

        public int ExecuteNonQuery()
        {
            var ps = ToIEnumerable(Parameters.GetEnumerator()).ToArray();
            return (int)Callback("DbCommandStub::ExecuteNonQuery(int)::CommandText", new ParentContext(CommandText, 0), ps);
        }

        public object ExecuteScalar()
        {
            var ps = ToIEnumerable(Parameters.GetEnumerator()).ToArray();
            return Callback("DbCommandStub::ExecuteScalar(object)::CommandText", new ParentContext(CommandText), ps);
        }
        public XmlReader ExecuteXmlReader()
        {
            var ps = ToIEnumerable(Parameters.GetEnumerator()).ToArray();
            return (XmlReader)Callback("DbCommandStub::ExecuteXmlReader(object)::CommandText", new ParentContext(CommandText), ps);
        }

        public void Prepare()
        {
            Callback("DbCommandStub::Prepare");
        }

        internal Task ExecuteNonQueryAsync()
        {
            ExecuteNonQuery();
            return Task.CompletedTask;
        }

        internal Task<IDataReader> ExecuteReaderAsync()
        {
            var ret = ExecuteReader();
            return Task.FromResult(ret);
        }

        internal Task<object> ExecuteScalarAsync()
        {
            var ret = ExecuteScalar();
            return Task.FromResult(ret);
        }

        internal Task<XmlReader> ExecuteXmlReaderAsync()
        {
            var ret = ExecuteXmlReader();
            return Task.FromResult(ret);
        }

        private IEnumerable<object> ToIEnumerable(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }

    public class DbDataParameterStub : IDbDataParameter
    {
        public string DataType { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }
        public DbType DbType { get; set; }
        public ParameterDirection Direction { get; set; }
        public bool IsNullable => false;
        public string ParameterName { get; set; }
        public string SourceColumn { get; set; }
        public DataRowVersion SourceVersion { get; set; }
        public object Value { get; set; }
        public override string ToString()
        {
            return $"{DataType ?? DbType.ToString()} {ParameterName}: {Value}";
        }
    }

    public class DataParameterCollectionStub : IDataParameterCollection
    {
        IDictionary<string, object> parameterDict = new Dictionary<string, object>();
        IList<object> parameterList = new List<object>();
        readonly SqlMockCallback Callback;
        readonly Func<string> CommandText;
        public DataParameterCollectionStub(SqlMockCallback callback, Func<string> commandText)
        {
            Callback = callback;
            CommandText = commandText;
        }

        public object this[string parameterName]
        {
            get
            {
                return Callback($"DataParameterCollectionStub::get_this(object)::parameterName", null, parameterName);
            }
            set
            {
                var index = this.IndexOf(parameterName);
                parameterList[index] = value;
                parameterDict[parameterName] = value;
                Callback($"DataParameterCollectionStub::set(object)::parameterName", null, value);
            }
        }
        public object this[int index]
        {
            get
            {

                return Callback($"DataParameterCollectionStub::get_this(object)::index", null, parameterList[index]);
            }
            set
            {
                var item = parameterList[index] as DbDataParameterStub;
                parameterList[index] = value;
                parameterDict.Remove(item.ParameterName);
                parameterDict.Add(((DbDataParameterStub)value).ParameterName, value);
                Callback($"DataParameterCollectionStub::set(object)::index", null, value);
            }
        }

        public bool IsFixedSize => (bool)Callback("DataParameterCollectionStub::IsFixedSize", true);

        public bool IsReadOnly => (bool)Callback("DataParameterCollectionStub::IsReadOnly", true);

        public int Count => (int)Callback("DataParameterCollectionStub::Count", parameterList.Count);

        public bool IsSynchronized => (bool)Callback("DataParameterCollectionStub::IsSynchronized", true);

        public object SyncRoot => Callback("DataParameterCollectionStub::SyncRoot", this);

        public int Add(object value)
        {
            var p = value as DbDataParameterStub;
            parameterDict.Add(p.ParameterName, p);
            parameterList.Add(p);
            return (int)Callback("DataParameterCollectionStub::Add::value", new ParentContext(CommandText(), parameterList.Count - 1), p);
        }

        public void Clear()
        {
            parameterList.Clear();
            parameterDict.Clear();
            Callback("DataParameterCollectionStub::Clear");
        }

        public bool Contains(string parameterName)
        {
            return (bool)Callback("DataParameterCollectionStub::Contains::parameterName", parameterDict.ContainsKey(parameterName), parameterName);
        }

        public bool Contains(object value)
        {
            var p = value as DbDataParameterStub;
            return (bool)Callback("DataParameterCollectionStub::Contains::value", parameterDict.ContainsKey(p.ParameterName), value);
        }

        public void CopyTo(Array array, int index)
        {
            Callback("DataParameterCollectionStub::CopyTo::array&index", null, array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)Callback("DataParameterCollectionStub::GetEnumerator(IEnumerator)", parameterList.GetEnumerator());
        }

        public int IndexOf(string parameterName)
        {
            var item = parameterList.FirstOrDefault(p => ((DbDataParameterStub)p).ParameterName == parameterName);
            var ret = -1;
            if(item != null)
            {
                ret = parameterList.IndexOf(item);
            }
            return (int)Callback("DataParameterCollectionStub::IndexOf(int)::parameterName", ret, parameterName);
        }

        public int IndexOf(object value)
        {
            var item = parameterList.FirstOrDefault(p => ((DbDataParameterStub)p).ParameterName == ((DbDataParameterStub)value).ParameterName);
            var ret = -1;
            if (item != null)
            {
                ret = parameterList.IndexOf(item);
            }
            return (int)Callback("DataParameterCollectionStub::IndexOf(int)::value", ret, value);
        }

        public void Insert(int index, object value)
        {
            parameterList.Insert(index, value);
            parameterDict.Add(((DbDataParameterStub)value).ParameterName, value);
            Callback("DataParameterCollectionStub::Insert::index&value", null, index, value);
        }

        public void Remove(object value)
        {
            var index = this.IndexOf(value);
            parameterList.RemoveAt(index);
            parameterDict.Remove(((DbDataParameterStub)value).ParameterName);
            Callback("DataParameterCollectionStub::Remove::value", null, value);
        }

        public void RemoveAt(string parameterName)
        {
            var index = this.IndexOf(parameterName);
            parameterList.RemoveAt(index);
            parameterDict.Remove(parameterName);
            Callback("DataParameterCollectionStub::RemoveAt::parameterName", null, parameterName);
        }

        public void RemoveAt(int index)
        {
            var item = parameterList[index];
            parameterList.RemoveAt(index);
            parameterDict.Remove(((DbDataParameterStub)item).ParameterName);
            Callback("DataParameterCollectionStub::RemoveAt::index", null, index);
        }
    }

    public class DataReaderStub : IDataReader
    {
        SqlMockCallback Callback;
        private readonly ParentContext parentContext;

        public DataReaderStub(SqlMockCallback callback, string parentContext, object[] dbParams)
        {
            Callback = callback;
            this.parentContext = new ParentContext(parentContext, null, dbParams);
        }

        public object this[int i] {
            get
            {
                return Callback("DataReaderStub::get_this(object)::i", parentContext, i);
            }
            set
            {
                Callback("DataReaderStub::set_this(object)::i", parentContext, i, value);
            }
        }

        public object this[string name]
        {
            get
            {
                return Callback("DataReaderStub::get_this(object)::name", parentContext, name);
            }
            set
            {
                Callback("DataReaderStub::set_this(object)::name", parentContext, name, value);
            }
        }

        public int Depth => (int)Callback("DataReaderStub::Depth");

        public bool IsClosed => (bool)Callback("DataReaderStub::IsClosed");

        public int RecordsAffected => (int)Callback("DataReaderStub::RecordsAffected");

        public int FieldCount => (int)Callback("DataReaderStub::FieldCount", parentContext);

        public void Close()
        {
            Callback("DataReaderStub::Close");
        }

        public void Dispose()
        {
            Callback("DataReaderStub::Dispose");
        }

        public bool GetBoolean(int i)
        {
            return (bool)Callback("DataReaderStub::GetBoolean(bool)::i", parentContext, i);
        }

        public byte GetByte(int i)
        {
            return (byte)Callback("DataReaderStub::GetByte(byte)::i", parentContext, i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return (long)Callback("DataReaderStub::GetBytes(long)::i&fieldoffset&buffer&bufferoffset&length", parentContext, i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return (char)Callback("DataReaderStub::GetChar(char)::i", parentContext, i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return (long)Callback("DataReaderStub::GetChars(long)::i&fieldoffset&buffer&bufferoffset&length", parentContext, i, fieldoffset, buffer, bufferoffset, length);
        }

        public IDataReader GetData(int i)
        {
            Callback("DataReaderStub::GetData(IDataReader)::i");
            return new DataReaderStub(Callback, parentContext.Name, parentContext.DbParams);
        }

        public string GetDataTypeName(int i)
        {
            return (string)Callback("DataReaderStub::GetDataTypeName(string)::i", parentContext, i);
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)Callback("DataReaderStub::GetDateTime(DateTime)::i", parentContext, i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)Callback("DataReaderStub::GetDecimal(decimal)::i", parentContext, i);
        }

        public double GetDouble(int i)
        {
            return (double)Callback("DataReaderStub::GetDouble(double)::i", parentContext, i);
        }

        //[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
        public Type GetFieldType(int i)
        {
            return (Type)Callback("DataReaderStub::GetFieldType(Type)::i", parentContext, i);
        }

        public float GetFloat(int i)
        {
            return (float)Callback("DataReaderStub::GetFloat(float)::i", parentContext, i);
        }

        public Guid GetGuid(int i)
        {
            return (Guid)Callback("DataReaderStub::GetGuid(Guid)::i", parentContext, i);
        }

        public short GetInt16(int i)
        {
            return (short)Callback("DataReaderStub::GetInt16(short)::i", parentContext, i);
        }

        public int GetInt32(int i)
        {
            return (int)Callback("DataReaderStub::GetInt32(int)::i", parentContext, i);
        }

        public long GetInt64(int i)
        {
            return (long)Callback("DataReaderStub::GetInt64(long)::i", parentContext, i);
        }

        public string GetName(int i)
        {
            return (string)Callback("DataReaderStub::GetName(string)::i", parentContext, i);
        }

        public int GetOrdinal(string name)
        {
            return (int)Callback("DataReaderStub::GetOrdinal(int)::name", parentContext, name);
        }

        public DataTable GetSchemaTable()
        {
            return (DataTable)Callback("DataReaderStub::GetSchemaTable(DataTable)", parentContext);
        }

        public string GetString(int i)
        {
            return (string)Callback("DataReaderStub::GetString(string)::i", parentContext, i);
        }

        public object GetValue(int i)
        {
            return Callback("DataReaderStub::GetValue(object)::i", parentContext, i);
        }

        public int GetValues(object[] values)
        {
            return (int)Callback("DataReaderStub::GetValues(int)::values", parentContext, values);
        }

        public bool IsDBNull(int i)
        {
            return (bool)Callback("DataReaderStub::IsDBNull(bool)::i", parentContext, i);
        }

        public bool NextResult()
        {
            return (bool)Callback("DataReaderStub::NextResult(bool)", parentContext);
        }

        public bool Read()
        {
            return (bool)Callback("DataReaderStub::Read(bool)", parentContext);
        }
    }

    public class DbTransactionStub : IDbTransaction
    {
        SqlMockCallback Callback;
        public IDbConnection Connection { get; set; }
        public IsolationLevel IsolationLevel { get; set; }
        public DbTransactionStub(SqlMockCallback callback)
        {
            Callback = callback;
        }

        public void Commit()
        {
            Callback("DbTransactionStub::Commit");
        }

        public void Dispose()
        {
            Callback("DbTransactionStub::Dispose");
        }

        public void Rollback()
        {
            Callback("DbTransactionStub::Rollback");
        }
    }
}
