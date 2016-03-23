using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate;
using NHibernate.Classic;

namespace UM007.NHibernate.TestDemo.Domain
{
    public class Role : ILifecycle, IValidatable
    {
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }

        public virtual LifecycleVeto OnSave(ISession s)
        {
            Console.WriteLine("您调用了Save()方法！");
            return LifecycleVeto.NoVeto;//LifecycleVeto.Veto若返回些值，则操作将被取消
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

        /// <summary>
        ///合法验证回调（IValidatable)是在持久化类在保存其持久化状态前进行合法性检查的接口 
        /// </summary>
        public virtual void Validate()
        {
            if (Id == "007")
            {
                throw new ValidationFailure("不允叫我的名字");
            }
        }
    }
}