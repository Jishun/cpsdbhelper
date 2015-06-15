using System.Xml.Linq;

namespace CpsDbHelper.Extensions
{
    public static class XmlExtension
    {
        /// <summary>
        /// Get an attribute from an XElement and cast it to type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this XElement src, string attributeName)
        {
            return (T)(object)src.Attribute(attributeName);
        }
    }
}
