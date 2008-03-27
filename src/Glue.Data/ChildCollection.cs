using System;
using System.Collections;
using Glue.Data;
using Glue.Data.Mapping;

namespace Glue.Data
{
	/// <summary>
	/// ChildCollection.
	/// </summary>
	public class ChildCollection
	{
        object _parent;
        Type _parentType;
        Type _childType;
        Entity _parentInfo;
        Entity _childInfo;
        EntityMember _foreignKey;
        Order _order;

        protected Entity ParentInfo 
        {
            get { return _parentInfo != null ? _parentInfo : _parentInfo = Entity.Obtain(_parentType); }
        }
        protected Entity ChildInfo 
        {
            get { return _childInfo != null ? _childInfo : _childInfo = Entity.Obtain(_childType); }
        }
        protected EntityMember ForeignKey
        {
            get { return _foreignKey != null ? _foreignKey : _foreignKey = ChildInfo.AllMembers.FindByColumnName(ParentInfo.Table.Name + "Id"); }
        }
        protected EntityMember PrimaryKey
        {
            get { return _parentInfo.KeyMembers[0]; }
        }
        public ChildCollection(object parent, Type childType) : this(parent, childType, null)
        {
        }
        public ChildCollection(object parent, Type childType, Order order)
		{
            _parent = parent;
            _parentType = parent.GetType();
            _childType = childType;
            _order = order;
		}
        
        public object New()
        {
            object child = Activator.CreateInstance(_childType);
            ForeignKey.SetValue(child, PrimaryKey.GetValue(_parent));
            return child;
        }
       public object Find(Filter filter)
        {
            IList list = List(filter, _order, Limit.One);
            return list != null && list.Count > 0 ? list[0] : null;
        }
        public IList List()
        {
            return List(null, _order, null);
        }
        public IList List(Filter filter)
        {
            return List(null, _order, null);
        }
        public IList List(Filter filter, Order order, Limit limit)
        {
            filter = Filter.And(filter, Filter.Create(ForeignKey.Column.Name + "=@0", PrimaryKey.GetValue(_parent)));
            return (IList)_childType.InvokeMember("List", System.Reflection.BindingFlags.Static, null, _childType, new object[] { filter, order, limit});
        }
    }
}
