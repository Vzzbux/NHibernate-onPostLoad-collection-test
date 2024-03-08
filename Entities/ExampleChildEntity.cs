using FluentNHibernate.Mapping;
using System;

namespace NHibernate.CollectionTest.Entities
{
    [Serializable]
    public class ExampleChildEntity : ISelfTrackingEntity
    {

        //Mapped properties
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual ExampleParentEntity Parent { get; protected set; }

        public ExampleChildEntity()
        {
        }

        /// <summary>
        /// Update bidirectional relationship.
        /// </summary>
        public virtual ExampleChildEntity SetParent(ExampleParentEntity parent)
        {
            Parent = parent;
            parent.ChildList.Add(this);
            return this;
        }

        public virtual void OnPostLoad()
        {
            Console.WriteLine("ExampleChildEntity OnPostLoad()");
            
        }

        public override string ToString()
        {
            return $"ExampleChildId [Id={Id}]";
        }
    }

    public class ExampleChildEntityMap : ClassMap<ExampleChildEntity>
    {
        public ExampleChildEntityMap()
        {
            Table("Children");
            Id(x => x.Id).GeneratedBy.Native();
            Map(x => x.Name).Length(256);
            References(x => x.Parent)
                .Column("Parent")
                .ForeignKey("CHILD_PARENT_FK")
                .UniqueKey("CHILD_NATURAL_UK");

        }
    }
}
