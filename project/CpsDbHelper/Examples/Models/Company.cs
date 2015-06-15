using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpsDbHelper.Examples.Models
{
    public class Company
    {
        public string Name { get; set; }
        public Address Address { get; set; }
        public string PropertyWithNoMatchingColumn { get; set; }
    }
}
