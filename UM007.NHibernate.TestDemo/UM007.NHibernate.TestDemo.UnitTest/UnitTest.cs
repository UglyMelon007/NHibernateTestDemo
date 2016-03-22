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
        [TestInitialize]
        public void Initializate()
        {

        }

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
            session.Delete("Person", new Person { Id = "1" });
            session.Flush();
        }
        #endregion

        #region NHibernateStateTransition
        /// <summary>
        /// 临时态-->持久态(Transient-》Persistent)
        /// </summary>
        [TestMethod]
        public void TransientToPersistent()
        {
            Configuration cfg = new Configuration().Configure("Config/MSSQL.cfg.xml");
            ISessionFactory factory = cfg.BuildSessionFactory();
            using (ISession session = factory.OpenSession())
            {
                //打开一个事务
                using (ITransaction tran = session.BeginTransaction())
                {
                    //transient临时态
                    Person person = new Person
                    {
                        Id = "2",
                        Name = "hello world"
                    };
                    try
                    {
                        //persistent持久态
                        session.Save(person);
                        //person处于persistent状态，没有脱离Session的管理，其属性值发生改变后，数据库相对应的记录会与之同步
                        person.Name = "world";
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();//若出错，则回滚
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 持久态-->游离态-->持久态(Persistent-》Detached-》Persistent
        /// </summary>
        [TestMethod]
        public void PersistenTranslateDetached()
        {
            Configuration cfg = new Configuration().Configure("Config/MSSQL.cfg.xml");
            ISessionFactory factory = cfg.BuildSessionFactory();

            //transient临时态
            Person person = new Person
            {
                Id = "007",
                Name = "hello world"
            };
            using (ISession session = factory.OpenSession())
            {
                //打开一个事务
                using (ITransaction tran = session.BeginTransaction())
                {
                    try
                    {
                        //persistent持久态
                        session.Save(person);
                        //person处于persistent状态，没有脱离Session的管理，其属性值发生改变后，数据库相对应的记录会与之同步
                        person.Name = "world";
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();//若出错，则回滚
                        throw ex;
                    }
                }
            }
            //Detached 游离状态
            person.Name = "hello world";
            using (ISession session = factory.OpenSession())
            {
                using (ITransaction tran = session.BeginTransaction())
                {
                    try
                    {
                        //Persistent 持久态
                        session.Update(person);
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                }
            }
        }
        #endregion
    }
}
