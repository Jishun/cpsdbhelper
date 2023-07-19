using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CpsDbHelper.Support
{
    public class DbProviderMockBuilder
    {
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

        public IDictionary<string, IList<IDictionary<string, object>>> dataReaderMapping = new Dictionary<string, IList<IDictionary<string, object>>>();
        public IDictionary<string, object> scalarMapping = new Dictionary<string, object>();
        public IDictionary<string, SqlMockCallback> contextHandlers = new Dictionary<string, SqlMockCallback>();
        public IDictionary<string, int> executionTracker = new Dictionary<string, int>();
        public IDictionary<string, int> readerExecutionTracker = new Dictionary<string, int>();

        public DbProviderMockBuilder AddReaderReturnData(string commandText, IDictionary<string, object> obj)
        {
            if(!dataReaderMapping.TryGetValue(commandText, out var list))
            {
                list = new List<IDictionary<string, object>>();
                dataReaderMapping.Add(commandText, list);
            }
            list.Add(obj);
            return this;
        }
        
        public DbProviderMockBuilder SetScalarReturnValue(string commandText, object value)
        {
            scalarMapping.Add(commandText, value);
            return this;
        }
        public DbProviderMockBuilder AddContextHandler(string context, SqlMockCallback callback)
        {
            contextHandlers.Add(context, callback);
            return this;
        }

        public AdoDotNetDataProviderStub Build()
        {
            return new AdoDotNetDataProviderStub((context, defaultRet, parameters) =>
            {
                var parentContext = defaultRet as ParentContext;
                if (contextHandlers.ContainsKey(context))
                {
                    return contextHandlers[context](context, defaultRet, parameters);
                }
                var command = parentContext?.Name;
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
                            return scalarMapping[command];
                        }
                        return defaultRet;
                    case Context_Reader_IsDbNull:
                        {
                            var index = executionTracker[command];
                            var key = $"{command}_{index}";

                            var dict = dataReaderMapping[command][readerExecutionTracker[key] - 1];
                            var columnKey = dict.Keys.ToList()[(int)parameters[0]];
                            return dict[columnKey] == null;
                        }
                    case Context_Reader_GetValue:
                        {
                            var index = executionTracker[command];
                            var key = $"{command}_{index}";

                            var dict = dataReaderMapping[parentContext.Name][readerExecutionTracker[key] - 1];
                            var columnKey = dict.Keys.ToList()[(int)parameters[0]];
                            return dict[columnKey];
                        }
                    case Context_Reader_GetOrdinal:
                        return dataReaderMapping[command][0].Keys.ToList().IndexOf((string)parameters[0]);
                    case Context_Reader_GetName:
                        return dataReaderMapping[command][0].Keys.ToList()[(int)parameters[0]];
                    case Context_Reader_Read:
                        {
                            var index = executionTracker[command];
                            var key = $"{command}_{index}";
                            var readCount = readerExecutionTracker[key];
                            readerExecutionTracker[key] = readCount + 1;
                            return dataReaderMapping[command].Count > readCount;
                        }
                    case Context_Reader_FieldCount:
                        return dataReaderMapping[command][0].Count;
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
