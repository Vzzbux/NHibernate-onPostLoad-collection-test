using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;

namespace NHibernate.CollectionTest.Entities
{

    [Serializable]
    public class ExampleParentEntity : ISelfTrackingEntity {

        //Mapped properties
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
        public virtual ISet<ExampleChildEntity> ChildList { get; protected internal set; }

        //Unmapped properties
        public virtual bool OnPostLoadWasCalled { get; protected internal set; }
        public virtual bool? ChildCollectionWasInitializedBeforeOnPostLoad { get; protected internal set; }
        public virtual string NameDuringOnPostLoad { get; protected internal set; }

        public ExampleParentEntity() {
            ChildList = new HashSet<ExampleChildEntity>();
            OnPostLoadWasCalled = false;
        }

        public virtual void OnPostLoad() {
            Console.WriteLine("ExampleParentEntity OnPostLoad()");
            OnPostLoadWasCalled = true;
            NameDuringOnPostLoad = Name;
            ChildCollectionWasInitializedBeforeOnPostLoad = NHibernateUtil.IsInitialized(ChildList);
        }

        public override string ToString() {
            return $"Entity:ExampleParentEntity [Id={Id}]";
        }

    }
    public class ExampleParentEntityMap : ClassMap<ExampleParentEntity>
    {
        public ExampleParentEntityMap()
        {
            Table("Parents");
            Id(x => x.Id).GeneratedBy.Assigned().Column("Id").Length(3);
            Map(x => x.Name).Length(256);
            // Owned by Child
            HasMany(x => x.ChildList)
                .KeyColumn("Parent")
                .Inverse()
                .Cascade.All()
                .LazyLoad();
        }
    }
}
