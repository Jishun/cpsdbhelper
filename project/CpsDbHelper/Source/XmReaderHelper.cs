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

        public XmlReaderHelper(string text, SqlConnection connection, SqlTransaction transaction)
            : base(text, connection, transaction)
        {
        }

        protected override void BeginExecute(SqlCommand cmd)
        {
            var ret = cmd.ExecuteXmlReader();
            _result = XElement.Load(ret);
        }

        protected override async Task BeginExecuteAsync(SqlCommand cmd)
        {
            var ret = await cmd.ExecuteXmlReaderAsync();
            _result = XElement.Load(ret);
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

}