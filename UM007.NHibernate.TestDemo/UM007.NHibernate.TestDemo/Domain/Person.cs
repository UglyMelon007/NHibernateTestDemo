using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UM007.NHibernate.TestDemo.Domain
{
    public class Person
    {
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
    }
}