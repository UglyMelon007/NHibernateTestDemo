using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Util;
using UM007.NHibernate.TestDemo.Domain;

namespace UM007.NHibernate.TestDemo.UnitTest
{
    [TestClass]
    public class UnitTest
    {
        private ISessionFactory _factory;

        [TestInitialize]
        public void Initializate()
        {
            Configuration cfg = new Configuration().Configure("Config/MSSQL.cfg.xml");
            _factory = cfg.BuildSessionFactory();
        }

        #region NHibernateCRUDTest
        [TestMethod]
        public void NHibernateAddUnitTest()
        {
            ISession session = _factory.OpenSession();
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
            ISession session = _factory.OpenSession();
            var temp = session.Get("Person", "1");
            session.Flush();
        }

        [TestMethod]
        public void NHibernateUpdateUnitTest()
        {
            ISession session = _factory.OpenSession();
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
            ISession session = _factory.OpenSession();
            session.Delete("Person", new Person { Id = "1" });
            session.Flush();
        }
        #endregion

        #region NHibernateStateTransition
        /// <summary>
        /// 临时态-->持久态(Transient-》Persistent)
        /// </summary>
        [TestMethod]
        public void TransientToPersistentUnitTest()
        {
            using (ISession session = _factory.OpenSession())
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
        public void PersistenTranslateDetachedUnitTest()
        {
            //transient临时态
            Person person = new Person
            {
                Id = "007",
                Name = "hello world"
            };
            using (ISession session = _factory.OpenSession())
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
            using (ISession session = _factory.OpenSession())
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

        #region NHibernateRelationMapTest

        #region (many-to-one)
        /// <summary>
        /// 多对一插入
        /// </summary>
        [TestMethod]
        public void ManyToOneCUnitTest()
        {
            using (ISession session = _factory.OpenSession())
            {
                Class cls = new Class { Id = "1" };
                Student std1 = new Student { Id = "3", Name = "叶某", Class = cls };
                Student std2 = new Student { Id = "4", Name = "韦某", Class = cls };
                using (ITransaction tran = session.BeginTransaction())
                {
                    try
                    {
                        session.Save(std1);
                        session.Save(std2);

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

        /// <summary>
        /// 多对一查询
        /// </summary>
        [TestMethod]
        public void MaynToOneRUnitTest()
        {
            using (ISession session = _factory.OpenSession())
            {
                string id = "1";
                Student st = session.CreateQuery("from Student where Id='1'").List<Student>().First() as Student;

                Console.WriteLine("人名{0}", st.Name);
                Console.WriteLine("班级名{0}", st.Class.Name);
            }
        }
        #endregion

        #region one-to-one

        #region 单向主键关系映射

        /// <summary>
        /// 单向主键关系映射插入测试
        /// </summary>
        [TestMethod]
        public void OneToOneMode1CUnitTest()
        {
            using (ISession session = _factory.OpenSession())
            {
                Student student = new Student { Id = "3" };
                Family family = new Family { Address = "中国", Student = student };

                using (ITransaction tran = session.BeginTransaction())
                {
                    try
                    {
                        session.Save(family);
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

        /// <summary>
        /// 单向主键关系映射查询测试
        /// </summary>
        [TestMethod]
        public void OneToOneMode1RUnitTest()
        {
            using (ISession session = _factory.OpenSession())
            {
                Family family = session.CreateQuery("from Family").List<Family>().First() as Family;

                Console.WriteLine("家庭地址为：{0}", family.Address);
                Console.WriteLine("学生姓名为：{0}", family.Student.Name);
            }
        }

        #endregion

        #region 双向主键关系映射

        /// <summary>
        /// 双向主键关系映射查询测试
        /// </summary>
        [TestMethod]
        public void OneToOneMode2RUnitTest()
        {
            using (ISession session = _factory.OpenSession())
            {
                Student student = session.Get<Student>("2");

                Console.WriteLine("学生姓名为：{0}", student.Name);
                Console.WriteLine("家庭地址为：{0}", student.Family.Address);
            }
        }
        #endregion

        #region 唯一外键关系映射

        /// <summary>
        /// 唯一外键关系映射插入测试
        /// </summary>
        [TestMethod]
        public void OneToOneMode3CUnitTest()
        {
            using (ISession session = _factory.OpenSession())
            {
                Teacher teacher = new Teacher { Id = "1", Name = "太一某某" };
                Class cls = session.Get<Class>("1");
                cls.Teacher = teacher;
                teacher.Class = cls;

                ITransaction tran = session.BeginTransaction();
                try
                {
                    session.Save(teacher);
                    session.Save(cls);

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }
        #endregion

        #endregion

        #region one-to-many

        /// <summary>
        /// 单向关联映射插入测试(和双向放一起了）
        /// </summary>
        [TestMethod]
        public void OneToManyCUnitTest()
        {
            using (ISession session = _factory.OpenSession())
            {
                Student qu = new Student { Id = "8", Name = "莫某某" };
                Student luo = new Student { Id = "9", Name = "君某某" };

                Class cls = session.Get<Class>("1");
                cls.Students = new List<Student> { qu, luo };
                qu.Class = cls;
                luo.Class = cls;

                ITransaction tran = session.BeginTransaction();
                try
                {

                    session.Save(cls);
                    session.Save(qu);
                    session.Save(luo);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 单向关联映射插入查询(和双向放一起了） 
        /// </summary>
        [TestMethod]
        public void OneToManyRUnitTest()
        {
            using (ISession session = _factory.OpenSession())
            {
                Class cls = session.Get<Class>("1");

                foreach (Student item in cls.Students)
                {
                    Console.WriteLine("学生名为：{0}", item.Name);
                    Console.WriteLine("班级名为：{0}", item.Class.Name);
                }
            }
        }

        #endregion

        #region many-to-many



        #endregion

        #endregion
    }
}
