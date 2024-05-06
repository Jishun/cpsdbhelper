using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CpsDbHelper.Support
{
    public class DbProviderMockBuilder
    {
        public delegate IDictionary<string, object> SqlReaderCallback(string context, int exeCount, int readCount = 0, IDictionary<string, object> paramers = null);
        public delegate object SqlScalarCallback(string context, int exeCount = 0, IDictionary<string, object> paramers = null);
        public delegate int SqlNonqueryCallback(string context, int exeCount = 0, IDictionary<string, object> paramers = null);

        public const string Context_ExecuteReader = "DbCommandStub::ExecuteReader(IDataReader)::CommandText";
        public const string Context_ExecuteReaderWithBehaviour = "DbCommandStub::ExecuteReader::CommandBehavior(IDataReader)::CommandText";
        public const string Context_ExecuteNonQuery = "DbCommandStub::ExecuteNonQuery(int)::CommandText";
        public const string Context_ExecuteScalar = "DbCommandStub::ExecuteScalar(object)::CommandText";
        public const string Context_ExecuteXmlReader = "DbCommandStub::ExecuteXmlReader(object)::CommandText";
        public const string Context_AddParameter = "DataParameterCollectionStub::Add::value";
        public const string Context_Reader_IsDbNull = "DataReaderStub::IsDBNull(bool)::i";
        public const string Context_Reader_GetValue = "DataReaderStub::GetValue(object)::i";
        public const string Context_Reader_GetOrdinal = "DataReaderStub::GetOrdinal(int)::name";
        public const string Context_Reader_GetName = "DataReaderStub::GetName(string)::i";
        public const string Context_Reader_Read = "DataReaderStub::Read(bool)";
        public const string Context_Reader_FieldCount = "DataReaderStub::FieldCount";
        public const string Context_default = "default";

        public IDictionary<string, SqlReaderCallback> dataReaderMapping = new Dictionary<string, SqlReaderCallback>();
        public IDictionary<string, SqlScalarCallback> scalarMapping = new Dictionary<string, SqlScalarCallback>();
        public IDictionary<string, SqlNonqueryCallback> nonqueryMapping = new Dictionary<string, SqlNonqueryCallback>();
        public IDictionary<string, SqlMockCallback> contextHandlers = new Dictionary<string, SqlMockCallback>();
        public IDictionary<string, SqlMockCallback> contextSubscribers = new Dictionary<string, SqlMockCallback>();
        public IDictionary<string, int> executionTracker = new Dictionary<string, int>();
        public IDictionary<string, int> readerExecutionTracker = new Dictionary<string, int>();

        public DbProviderMockBuilder SetReaderCallback(string commandText, SqlReaderCallback obj)
        {
            dataReaderMapping.Add(commandText, obj);
            return this;
        }
        
        public DbProviderMockBuilder SetScalarCallback(string commandText, SqlScalarCallback value)
        {
            scalarMapping.Add(commandText, value);
            return this;
        }
        public DbProviderMockBuilder SetNonqueryCallback(string commandText, SqlNonqueryCallback value)
        {
            nonqueryMapping.Add(commandText, value);
            return this;
        }
        public DbProviderMockBuilder AddContextHandler(string context, SqlMockCallback callback)
        {
            contextHandlers.Add(context, callback);
            return this;
        }
        public DbProviderMockBuilder AddContextSubscriber(string context, SqlMockCallback callback)
        {
            contextSubscribers.Add(context, callback);
            return this;
        }

        public AdoDotNetDataProviderStub Build()
        {
            return new AdoDotNetDataProviderStub((context, defaultRet, parameters) =>
            {
                var parentContext = defaultRet as ParentContext;
                var command = parentContext?.Name;
                var dbParams = parentContext?.DbParams?.Cast<DbDataParameterStub>().ToDictionary(p => p.ParameterName.ToLower().Replace("@", ""), p => p.Value);
                switch (context)
                {
                    case Context_ExecuteReader:
                    case Context_ExecuteReaderWithBehaviour:
                    case Context_ExecuteNonQuery:
                    case Context_ExecuteXmlReader:
                    case Context_ExecuteScalar:
                        executionTracker.TryGetValue(command, out int count);
                        executionTracker[command] = count + 1;
                        break;
                }
                if (contextSubscribers.ContainsKey(context))
                {
                    contextSubscribers[context](context, defaultRet, parameters);
                }
                if (contextHandlers.ContainsKey(context))
                {
                    return contextHandlers[context](context, defaultRet, parameters);
                }
                switch (context)
                {
                    case Context_ExecuteReader:
                    case Context_ExecuteReaderWithBehaviour:
                        {
                            var index = executionTracker[command];
                            var key = $"{command}_{index}";
                            readerExecutionTracker[key] = 0;
                            return defaultRet;
                        }
                    case Context_ExecuteScalar:
                        if (scalarMapping.ContainsKey(command))
                        {
                            return scalarMapping[command](context, executionTracker[command], dbParams);
                        }
                        return defaultRet;
                    case Context_ExecuteNonQuery:
                        if (nonqueryMapping.ContainsKey(command))
                        {
                            return nonqueryMapping[command](context, executionTracker[command], dbParams);
                        }
                        return defaultRet;
                    case Context_Reader_IsDbNull:
                        {
                            var index = executionTracker[command];
                            var key = $"{command}_{index}";
                            var dict = dataReaderMapping[command](context, index, readerExecutionTracker[key], dbParams);
                            var columnKey = dict.Keys.ToList()[(int)parameters[0]];
                            return dict[columnKey] == null;
                        }
                    case Context_Reader_GetValue:
                        {
                            var index = executionTracker[command];
                            var key = $"{command}_{index}";
                            var dict = dataReaderMapping[command](context, index, readerExecutionTracker[key], dbParams);
                            var columnKey = dict.Keys.ToList()[(int)parameters[0]];
                            return dict[columnKey];
                        }
                    case Context_Reader_GetOrdinal:
                        return dataReaderMapping[command](context, executionTracker[command], 0, dbParams).Keys.Select(k => k.ToLowerInvariant()).ToList().IndexOf((string)parameters[0]);
                    case Context_Reader_GetName:
                        return dataReaderMapping[command](context, executionTracker[command], 0, dbParams).Keys.ToList()[(int)parameters[0]];
                    case Context_Reader_Read:
                        {
                            var index = executionTracker[command];
                            var key = $"{command}_{index}";
                            var readCount = readerExecutionTracker[key];
                            readerExecutionTracker[key] = readCount + 1;
                            return dataReaderMapping[command](context, index, readCount + 1, dbParams) != null;
                        }
                    case Context_Reader_FieldCount:
                        return dataReaderMapping[command](context, executionTracker[command], 0, dbParams).Count;
                    default:
                        if (contextHandlers.ContainsKey(Context_default))
                        {
                            return contextHandlers[Context_default](context, defaultRet, parameters);
                        }
                        return parentContext?.DefaultRet ?? defaultRet;
                }
            });
        }
    }

}
