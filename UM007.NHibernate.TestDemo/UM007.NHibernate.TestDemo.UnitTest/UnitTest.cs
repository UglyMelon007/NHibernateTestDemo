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
                    catch (Exception)
                    {
                        tran.Rollback();//若出错，则回滚
                        throw;
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
                    catch (Exception)
                    {
                        tran.Rollback();//若出错，则回滚
                        throw;
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
                    catch (Exception)
                    {
                        tran.Rollback();
                        throw;
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
                    catch (Exception)
                    {
                        tran.Rollback();
                        throw;
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
                    catch (Exception)
                    {
                        tran.Rollback();
                        throw;
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
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
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
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
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

        #region 单向多对多

        /// <summary>
        /// 单向多对多插入测试
        /// </summary>
        [TestMethod]
        public void ManyToManyModel1CUnitTest()
        {
            using (ISession session = _factory.OpenSession())
            {
                Role role1 = new Role { Id = "1", Name = "库管" };
                Role role2 = new Role { Id = "2", Name = "出纳" };
                Role role3 = new Role { Id = "3", Name = "会计" };

                User liu = new User
                {
                    Id = "1",
                    Name = "刘四",
                    Roles = new List<Role> { role1, role2 }
                };
                User zhang = new User
                {
                    Id = "2",
                    Name = "张三",
                    Roles = new List<Role> { role2, role3 }
                };

                ITransaction tran = session.BeginTransaction();
                try
                {
                    session.Save(role1);
                    session.Save(role2);
                    session.Save(role3);

                    session.Save(liu);
                    session.Save(zhang);

                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region 双向多对多

        /// <summary>
        /// 双向多对多插入测试
        /// </summary>
        [TestMethod]
        public void ManyToManyModel2CUntilTest()
        {
            using (ISession session = _factory.OpenSession())
            {
                var user = new User
                {
                    Id = "3",
                    Name = "刘五"
                };

                var role = new Role
                {
                    Id = "4",
                    Name = "系统管理员",
                    Users = new List<User> { user }
                };

                ITransaction tran = session.BeginTransaction();
                try
                {
                    session.Save(user);
                    session.Save(role);

                    tran.Commit();
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 双向多对多更新测试
        /// </summary>
        [TestMethod]
        public void ManyToManyModel2UUnitTest()
        {
            using (ISession session = _factory.OpenSession())
            {

                var role = session.Get<Role>("4");
                var user = session.Get<User>("1");
                user.Roles.Clear();
                user.Roles.Add(role);

                ITransaction tran = session.BeginTransaction();
                try
                {
                    session.Update(user);

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
        /// 双向多对多查询测试
        /// </summary>
        [TestMethod]
        public void ManyToManyModel2RUnitTest()
        {
            using (ISession session = _factory.OpenSession())
            {
                var user = session.Get<User>("1");

                foreach (var item in user.Roles)
                {
                    Console.WriteLine("角色名称：{0}", item.Name);
                    foreach (var us in item.Users)
                    {
                        Console.WriteLine("用户名称：{0}", us.Name);
                    }
                }
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
