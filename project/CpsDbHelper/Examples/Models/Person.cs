using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpsDbHelper.Examples.Models
{
    internal enum Gender
    {
        Male = 0,
        Female = 1
    }

    internal class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Gender Gender { get; set; }
    }
}
