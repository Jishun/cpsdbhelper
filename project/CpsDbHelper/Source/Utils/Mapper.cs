using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CpsDbHelper.Utils
{
    public abstract class Mapper<T> where T : Mapper<T>
    {
        private BindingFlags? _bindingFlag;

        public static BindingFlags DefaultGetBindingFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty;
        public static BindingFlags DefaultSetBindingFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty;

        public BindingFlags BindingFlag
        {
            get { return _bindingFlag ?? (ForGet ? DefaultGetBindingFlag : DefaultSetBindingFlag); }
            set { _bindingFlag = value; }
        }

        protected virtual bool ForGet => true;
        
        /// <summary>
        /// Specify binding flag to be used to get member values before using AutoMap
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public virtual T WithBindingFlag(BindingFlags flag)
        {
            BindingFlag = flag;
            return (T)this;
        }

        /// <summary>
        /// Set binding flag to get all public field values before using AutoMap
        /// </summary>
        /// <returns></returns>
        public virtual T WithAllPublicFileds()
        {
            BindingFlag = BindingFlags.Instance | BindingFlags.Public | (ForGet ? BindingFlags.GetField : BindingFlags.SetField);
            return (T)this;
        }

        /// <summary>
        /// Set binding flag to get all public properties values before using AutoMap
        /// </summary>
        /// <returns></returns>
        public virtual T WithAllPublicProperties()
        {
            BindingFlag = BindingFlags.Instance | BindingFlags.Public | (ForGet ? BindingFlags.GetProperty: BindingFlags.SetProperty);
            return (T)this;
        }

    }
}
