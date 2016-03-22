using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate;
using NHibernate.Classic;

namespace UM007.NHibernate.TestDemo.Domain
{
    public class Catalog : ILifecycle
    {
        public virtual string Id { get; set; }

        public virtual string PersonId { get; set; }

        public virtual LifecycleVeto OnSave(ISession s)
        {
            Console.WriteLine("您调用了Save()方法！");
            return LifecycleVeto.Veto;//LifecycleVeto.Veto若返回些值，则操作将被取消
        }

        public virtual LifecycleVeto OnUpdate(ISession s)
        {
            Console.WriteLine("您调用了Update()方法！");
            return LifecycleVeto.NoVeto;
        }

        public virtual LifecycleVeto OnDelete(ISession s)
        {
            Console.WriteLine("您调用了Delete()方法！");
            return LifecycleVeto.NoVeto;
        }

        public virtual void OnLoad(ISession s, object id)
        {
            Console.WriteLine("您调用了Load()方法！");
        }
    }
}