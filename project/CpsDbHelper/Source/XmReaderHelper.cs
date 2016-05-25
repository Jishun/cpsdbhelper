using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Xml.Linq;
using CpsDbHelper.Utils;

namespace CpsDbHelper
{
    public class XmlReaderHelper : DbHelper<XmlReaderHelper>
    {
        private XElement _result = null;
        public XmlReaderHelper(string text, string connectionString)
            : base(text, connectionString)
        {
        }

        public XmlReaderHelper(string text, IDbConnection connection, IDbTransaction transaction)
            : base(text, connection, transaction)
        {
        }

        protected override void BeginExecute(IDbCommand cmd)
        {
            var command = cmd as SqlCommand;
            if (command != null)
            {
                var ret = command.ExecuteXmlReader();
                _result = XElement.Load(ret);
            }
            else
            {
                throw new NotSupportedException("The xml operation is not supported by this data provider");
            }
        }

        protected override async Task BeginExecuteAsync(IDbCommand cmd)
        {
            var command = cmd as SqlCommand;
            if (command != null)
            {
                var ret = await command.ExecuteXmlReaderAsync();
                _result = XElement.Load(ret);
            }
            else
            {
                throw new NotSupportedException("The xml async operation is not supported by this data provider");
            }
        }

        public XmlReaderHelper GetResult(out XElement result)
        {
            result = _result;
            return this;
        }

        public XElement GetResult()
        {
            return _result;
        }

        /// <summary>
        /// Experimental
        /// Begin mapping the TValue's properties with the xml's attributes/elements
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public XmlMapper<TValue> BeginMapXml<TValue>()
        {
            return new XmlMapper<TValue>();
        }

        /// <summary>
        /// Experimental
        /// Auto map the TValue's properties with the xml's attributes, collections will be mapped with elements(not implemented)
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public TValue AutoMapXml<TValue>(XElement src)
        {
            return new XmlMapper<TValue>().AutoMap().FinishMap(src);
        }
    }

    public static partial class FactoryExtensions
    {
        public static XmlReaderHelper BeginXmlReader(this DbHelperFactory factory, string text)
        {
            if (factory.CheckExistingConnection())
            {
                return new XmlReaderHelper(text, factory.DbConnection, factory.DbTransaction);
            }
            return new XmlReaderHelper(text, factory.ConnectionString);
        }
    }
}