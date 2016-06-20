using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;

namespace CpsDbHelper.Utils
{
    /// <summary>
    /// Experimental. collection not supported
    /// untested code
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class XmlMapper<TValue> : Mapper<XmlMapper<TValue>>
    {
        private readonly IDictionary<Type, IDictionary<string, string>> _attributeMapper = new Dictionary<Type, IDictionary<string, string>>();
        private readonly IDictionary<Type, IDictionary<string, string>> _elementMapper = new Dictionary<Type, IDictionary<string, string>>();
        private readonly IDictionary<Type, HashSet<string>> _skip = new Dictionary<Type, HashSet<string>>();
        private readonly IList<Action<TValue, XElement>> _customMapper = new List<Action<TValue, XElement>>();

        protected override bool ForGet => false;

        /// <summary>
        /// Map a property name to another attribute name
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="attributeName">target attribute name</param>
        /// <param name="type">the type which the property belongs to</param>
        /// <returns></returns>
        public XmlMapper<TValue> MapAttribute(string propertyName, string attributeName, Type type = null)
        {
            type = type ?? typeof(TValue);
            var dict = GetTypeAttributeDict(type);
            dict.Add(propertyName, attributeName);
            return this;
        }

        /// <summary>
        /// Map a property to an element
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="elementName">target element name</param>
        /// <param name="type">the type which the property belongs to</param>
        /// <returns></returns>
        public XmlMapper<TValue> MapElement(string propertyName, string elementName, Type type = null)
        {
            type = type ?? typeof (TValue);
            var dict = GetTypeElementDict(type);
            dict.Add(propertyName, elementName);
            return this;
        }

        /// <summary>
        /// Manually operate the reuslt XElement
        /// </summary>
        /// <param name="func">the action to operate on the result XElement</param>
        /// <returns></returns>
        public XmlMapper<TValue> Map(Action<TValue, XElement> func)
        {
            _customMapper.Add(func);
            return this;
        }

        /// <summary>
        /// Manually operate the reuslt XElement
        /// </summary>
        /// <param name="func">the action to operate the result XElement</param>
        /// <param name="type">the type that this mapp applies to</param>
        /// <returns></returns>
        public XmlMapper<TValue> AddMap(Action<TValue, XElement> func, Type type = null)
        {
            type = type ?? typeof(TValue);
            if (!_skip.ContainsKey(type))
            {
                _skip.Add(type, new HashSet<string>());
            }
            _customMapper.Add(func);
            return this;
        }

        /// <summary>
        /// Skip specific property by name
        /// </summary>
        /// <param name="type">the type of this defination to apply to</param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public XmlMapper<TValue> Skip(Type type, string propertyName)
        {
            var hash = GetSkipHashset(type);
            hash.Add(propertyName);
            return this;
        }

        /// <summary>
        /// Try auto map the type's all properties into attribute, collections will be mapped into children elements(not implemented)
        /// </summary>
        /// <param name="type">the type to map</param>
        /// <returns></returns>
        public XmlMapper<TValue> AutoMap(Type type = null)
        {
            type = type ?? typeof (TValue);
            var skip = _skip.ContainsKey(type) ? _skip[type] : new HashSet<string>();
            var attibutes = GetTypeAttributeDict(type);
            var elements = GetTypeElementDict(type);
            var properties = type.GetProperties(BindingFlag).Where(p => p.CanWrite && !skip.Contains(p.Name));
            foreach (var p in properties)
            {
                if (p.PropertyType.IsPrimitive || p.PropertyType == typeof(string))
                {
                    attibutes.Add(p.Name, p.Name);
                }
                else
                {
                    elements.Add(p.Name, p.Name);
                    AutoMap(p.PropertyType);
                }
            }
            return this;
        }

        /// <summary>
        /// Finish mapping and use the src to construct XElement
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public TValue FinishMap(XElement src)
        {
            if (src == null)
            {
                return default(TValue);
            }

            return (TValue)Deserialize(typeof(TValue), src);
        }

        private object Deserialize(Type type, XElement src)
        {
            if (src == null)
            {
                return null;
            }
            var skip = _skip.ContainsKey(type) ? _skip[type] : new HashSet<string>();
            var attributes = _attributeMapper.ContainsKey(type) ? _attributeMapper[type] : new Dictionary<string, string>();
            var elements = _elementMapper.ContainsKey(type) ? _elementMapper[type] : new Dictionary<string, string>();
            var properties = type.GetProperties(BindingFlag).Where(p => p.CanWrite && !skip.Contains(p.Name));
            var item = Activator.CreateInstance(type);
            foreach (var p in properties)
            {
                if (attributes.ContainsKey(p.Name))
                {
                    if (p.PropertyType.IsAssignableFrom(typeof(XAttribute)))
                    {
                        p.SetValue(item, src.Attribute(attributes[p.Name]));
                    }
                    else
                    {
#if DEBUG
                        throw new NotImplementedException("attribute type not deserializable");
#endif
                    }
                }
                else if (elements.ContainsKey(p.Name))
                {
                    p.SetValue(item, Deserialize(p.PropertyType, src.Element(elements[p.Name])));
                }
            }
            return item;
        }

        private HashSet<string> GetSkipHashset(Type type)
        {

            if (!_skip.ContainsKey(type))
            {
                _skip.Add(type, new HashSet<string>());
            }
            return _skip[type];
        }

        private IDictionary<string, string> GetTypeAttributeDict(Type type)
        {
            if (!_attributeMapper.ContainsKey(type))
            {
                _attributeMapper.Add(type, new Dictionary<string, string>());
            }
            return _attributeMapper[type];
        }

        private IDictionary<string, string> GetTypeElementDict(Type type)
        {
            if (!_elementMapper.ContainsKey(type))
            {
                _elementMapper.Add(type, new Dictionary<string, string>());
            }
            return _elementMapper[type];
        }

    }
}
