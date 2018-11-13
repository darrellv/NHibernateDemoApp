using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Cache;

using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernateDemoApp
{
    class Program
    {

        static NHibernate.ISessionFactory GetSessionFactory()
        {
            var cfg = new Configuration();

            cfg.DataBaseIntegration(x => {
                x.ConnectionString = "Data Source=IAS-LP131\\SQLExpress;Initial Catalog=NHibernateDemoDB;Integrated Security=True;Pooling=False";
                x.Driver<SqlClientDriver>();
                x.Dialect<MsSql2008Dialect>();
                x.LogSqlInConsole = true;
                x.BatchSize = 10;
            });

            //caching code
            cfg.Cache(c =>
            {
                c.UseMinimalPuts = true;
                c.UseQueryCache = true;
            });

            cfg.SessionFactory()
                .Caching
                .Through<HashtableCacheProvider>()
                .WithDefaultExpiration(1440);

            cfg.AddAssembly(Assembly.GetExecutingAssembly());

            NHibernate.ISessionFactory session = cfg.BuildSessionFactory();

            return session;

        }
        static void SaveSomeStudents()
        {
            NHibernate.ISessionFactory sefact = GetSessionFactory();

            using (NHibernate.ISession session = sefact.OpenSession())
            {

                using (NHibernate.ITransaction tx = session.BeginTransaction())
                {
                    //perform database logic 
                    var student1 = new Student
                    {
                        FirstName = "Allan",
                        LastName = "Bommer",
                        AcademicStanding = Student.StudentAcademicStanding.Excellent 
                    };

                    var student2 = new Student
                    {
                        FirstName = "Jerry",
                        LastName = "Lewis",
                        AcademicStanding = Student.StudentAcademicStanding.Good 
                    };

                    session.Save(student1);
                    session.Save(student2);

                    tx.Commit();
                }

                //Console.ReadLine();
            }

        }

        static void WriteOutData(Student student)
        {
            Console.WriteLine("{0} \t{1} \t{2} \t{3}", student.ID, student.FirstName, student.LastName, student.AcademicStanding);
        }

        static void ShowAllStudents(NHibernate.ISession session)
        {

            IList<Student> students = session.CreateCriteria<Student>().List<Student>();

            foreach (Student student in students)
            {
                WriteOutData(student);
            }

        }

        static void ShowStudentData()
        {
            NHibernate.ISessionFactory sefact = GetSessionFactory();

            using (NHibernate.ISession session = sefact.OpenSession())
            {
                using (NHibernate.ITransaction tx = session.BeginTransaction())
                {
                    ShowAllStudents(session);

                    tx.Commit();
                }
            }

            Console.ReadLine();

        }

        static void GetOneStudent(int id)
        {
            NHibernate.ISessionFactory sefact = GetSessionFactory();

            using (NHibernate.ISession session = sefact.OpenSession())
            {
                using (NHibernate.ITransaction tx = session.BeginTransaction())
                {
                    ShowAllStudents(session);

                    Student stdnt = session.Get<Student>(id);

                    Console.WriteLine("Retrieved by ID");
                    WriteOutData(stdnt);

                    tx.Commit();
                }
            }

            Console.ReadLine();
        }


        static void UpdateRecord(int id, string lName)
        {
            NHibernate.ISessionFactory seFact = GetSessionFactory();

            using (NHibernate.ISession session = seFact.OpenSession())
            {
                using (NHibernate.ITransaction tx = session.BeginTransaction())
                {
                    ShowAllStudents(session);

                    Student stdnt = session.Get<Student>(id);

                    Console.WriteLine("Retrieved by ID");
                    WriteOutData(stdnt);

                    Console.WriteLine("Update the last name of ID = {0}", stdnt.ID);
                    stdnt.LastName = lName;
                    session.Update(stdnt);

                    Console.WriteLine("\nFetch the complete list again\n");

                    ShowAllStudents(session);
                    tx.Commit();
                }
            }
            Console.ReadLine();
        }

        static void DeleteRecord(int id)
        {
            NHibernate.ISessionFactory seFact = GetSessionFactory();

            using (NHibernate.ISession session = seFact.OpenSession())
            {
                using (NHibernate.ITransaction tx = session.BeginTransaction())
                {
                    ShowAllStudents(session);

                    Student stdnt = session.Get<Student>(id);

                    Console.WriteLine("Retrieved by ID");
                    WriteOutData(stdnt);

                    Console.WriteLine("Delete the record which has ID = {0}", stdnt.ID);
                    session.Delete(stdnt);

                    Console.WriteLine("\nFetch the complete list again\n");

                    ShowAllStudents(session);

                    tx.Commit();
                }
            }

            Console.ReadLine();
        }

        private static void Insert25StudentsToShowBatchCount()
        {
            NHibernate.ISessionFactory sefact = GetSessionFactory();

            using (NHibernate.ISession session = sefact.OpenSession())
            {

                using (NHibernate.ITransaction tx = session.BeginTransaction())
                {
                    for (int i = 0; i < 25; i++)
                    {

                        var student = new Student
                        {
                            FirstName = "FirstName" + i.ToString(),
                            LastName = "LastName" + i.ToString(),
                            AcademicStanding = Student.StudentAcademicStanding.Good
                        };

                        session.Save(student);
                    }

                    tx.Commit();
                }

                //Console.ReadLine();
            }
        }

        private static void GetCachedStudent(Guid guid)
        {
            NHibernate.ISessionFactory sefact = GetSessionFactory();

            using (NHibernate.ISession session = sefact.OpenSession())
            {
                using (NHibernate.ITransaction tx = session.BeginTransaction())
                {

                    Student stdntFirstQuery = session.Get<Student>(guid);

                    //this one is retrieved from the cache!
                    Student stdntSecondQuery = session.Get<Student>(guid);

                    Console.WriteLine("Retrieved by ID");
                    WriteOutData(stdntFirstQuery);
                    WriteOutData(stdntSecondQuery);

                    tx.Commit();
                }
            }

            Console.ReadLine();
        }

        static void Main(string[] args)
        {App_Start.NHibernateProfilerBootstrapper.PreStart();

            //Insert25StudentsToShowBatchCount();
            //SaveSomeStudents();
            //ShowStudentData();
            //GetOneStudent(1);

            //get cashed student
            GetCachedStudent(new Guid("cd0e9281-15dd-4da1-9c64-a9970145d33a"));

            //UpdateRecord(1, "Donald");
            //DeleteRecord(2);
        }

    }
}

