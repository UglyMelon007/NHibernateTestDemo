using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using UM007.NHibernate.TestDemo.Domain;

namespace UM007.NHibernate.TestDemo.UnitTest
{
    [TestClass]
    public class UnitTest
    {
        #region NHibernateCRUDTest
        [TestMethod]
        public void NHibernateAddUnitTest()
        {
            Configuration cfg = new Configuration().Configure("Config/MSSQL.cfg.xml");
            ISessionFactory factory = cfg.BuildSessionFactory();
            ISession session = factory.OpenSession();
            Person person = new Person
            {
                Id = "1",
                Name = "hello"
            };
            session.Save(person);
            session.Flush();
        }

        [TestMethod]
        public void NHibernateSelectUnitTest()
        {
            Configuration cfg = new Configuration().Configure("Config/MSSQL.cfg.xml");
            ISessionFactory factory = cfg.BuildSessionFactory();
            ISession session = factory.OpenSession();
            var temp = session.Get("Person", "1");
            session.Flush();
        }

        [TestMethod]
        public void NHibernateUpdateUnitTest()
        {
            Configuration cfg = new Configuration().Configure("Config/MSSQL.cfg.xml");
            ISessionFactory factory = cfg.BuildSessionFactory();
            ISession session = factory.OpenSession();
            Person person = new Person
            {
                Id = "1",
                Name = "world`l"
            };
            session.Update("Person", person);
            session.Flush();
        }

        [TestMethod]
        public void NHibernateDeleteUnitTest()
        {
            Configuration cfg = new Configuration().Configure("Config/MSSQL.cfg.xml");
            ISessionFactory factory = cfg.BuildSessionFactory();
            ISession session = factory.OpenSession();
            session.Delete("Person", new Person {Id = "1"});
            session.Flush();
        }
        #endregion

    }
}
