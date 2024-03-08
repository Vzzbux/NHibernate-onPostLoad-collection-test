using FluentAssertions;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.CollectionTest.Entities;
using NHibernate.Event;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using System.Linq;

namespace NHibernate.CollectionTest
{
    [TestFixture]
    public class TestOnPostLoad
    {
        private ISession _session;
        private const string TestQuery = "select distinct p from ExampleParentEntity p inner join fetch p.ChildList";

        [SetUp]
        public void SetUp()
        {
            log4net.Config.XmlConfigurator.Configure();

            var fluentConfiguration = Fluently.Configure()
                .Database(
                    SQLiteConfiguration.Standard
                        .InMemory()
                        .ShowSql()
                        .ConnectionString(cs => cs.Is("Data Source=:memory:;Version=3;New=True;DateTimeFormatString=yyyy-MM-dd HH:mm:ss.FFFFFFF"))
                )
                .Mappings(m =>
                {
                    m.FluentMappings
                        .AddFromAssemblyOf<ExampleParentEntity>();
                    m.HbmMappings
                        .AddFromAssemblyOf<ExampleParentEntity>();
                });

            //Wire up the PostLoadEvent listener
            NHibernate.Cfg.Configuration configuration = null;
            fluentConfiguration.ExposeConfiguration(config =>
            {
                config.EventListeners.PostLoadEventListeners = new IPostLoadEventListener[] {
                    new OnLoadEventListener()
                };
                configuration = config;
            });

            var sessionFactory = fluentConfiguration.BuildSessionFactory();
            var schemaExport = new SchemaExport(configuration);

            _session = sessionFactory.OpenSession();

            //Populate the schema
            schemaExport.Execute(true, true, false, _session.Connection, null);
        }

        [TearDown]
        public void TearDown()
        {
            _session.Dispose();
        }
        private ExampleParentEntity A_parent_object_with_a_single_child_is_persisted()
        {
            ExampleParentEntity parent;
            using (var tx = _session.BeginTransaction())
            {
                parent = new ExampleParentEntity()
                {
                    Id = "001",
                    Name = "Parent 1"
                };
                new ExampleChildEntity()
                {
                    Name = "Child 1"
                }.SetParent(parent);

                _session.Save(parent);
                tx.Commit();
            }
            _session.Clear();
            return parent;
        }

        [Test]
        public void ChildCollection_has_been_initialized_after_Parent_OnPostLoad_called()
        {
            // Given
            A_parent_object_with_a_single_child_is_persisted();

            // When
            var query = _session.CreateQuery(TestQuery);
            var results = query.List<ExampleParentEntity>();

            // Then
            results.Should().HaveCount(1);
            var parent = results.First();
            parent.Id.Should().Be("001");
            NHibernateUtil.IsInitialized(parent.ChildList).Should().BeTrue();

            //Was parent.ChildList correctly hydrated after the object was returned from query.List<ExampleParentEntity>()?
            parent.ChildList.Should().HaveCount(1);
            parent.ChildList.First().Name.Should().Be("Child 1");
        }

        [Test]
        public void Primitive_type_has_been_initialized_before_Parent_OnPostLoad_called()
        {
            // Given
            A_parent_object_with_a_single_child_is_persisted();

            // When
            var query = _session.CreateQuery(TestQuery);
            var results = query.List<ExampleParentEntity>();

            // Then
            results.Should().HaveCount(1);
            var parent = results.First();
            parent.Id.Should().Be("001");
            parent.OnPostLoadWasCalled.Should().BeTrue();

            //Was the Name correctly hydrated before parent.OnPostLoad() was called?
            parent.NameDuringOnPostLoad.Should().Be("Parent 1");
        }

        [Test]
        public void ChildCollection_has_been_initialized_before_Parent_OnPostLoad_called()
        {
            // Given
            A_parent_object_with_a_single_child_is_persisted();

            // When
            var query = _session.CreateQuery(TestQuery);
            var results = query.List<ExampleParentEntity>();

            // Then
            results.Should().HaveCount(1);
            var parent = results.First();
            parent.Id.Should().Be("001");
            parent.OnPostLoadWasCalled.Should().BeTrue();

            //Was parent.ChildList correctly hydrated before parent.OnPostLoad() was called?
            parent.ChildCollectionWasInitializedBeforeOnPostLoad.Should().BeTrue();
        }
    }
}
