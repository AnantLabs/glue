using System;
using System.Collections.Generic;
using System.Text;

namespace Glue.Data.Mapping
{
    //public class OneToManyAttribute
    //{
    //}
    
    //public class ManyToManyAttribute
    //{
    //}

    public class ManyToManyInfo
    {
        public Type LeftType;
        public Entity LeftInfo;
        public string LeftTable;
        public string LeftKey;
        public EntityMember LeftKeyInfo;
        public Type RightType;
        public Entity RightInfo;
        public string RightTable;
        public string RightKey;
        public EntityMember RightKeyInfo;
        public string JoinTable;
        public string JoinLeftKey;
        public string JoinRightKey;

        public ManyToManyInfo(Type left, Type right, string jointable)
        {
            LeftType = left;
            LeftInfo = Entity.Obtain(left);
            LeftTable = LeftInfo.Table.Name;
            LeftKeyInfo = LeftInfo.KeyMembers[0];
            LeftKey = LeftKeyInfo.Column.Name;
            RightType = right;
            RightInfo = Entity.Obtain(right);
            RightTable = RightInfo.Table.Name;
            RightKeyInfo = RightInfo.KeyMembers[0];
            RightKey = RightKeyInfo.Column.Name;
            JoinTable = jointable; // LeftTable + "_" + RightTable;
            JoinLeftKey = LeftKey.StartsWith(LeftTable, StringComparison.OrdinalIgnoreCase) ? LeftKey : LeftTable + LeftKey;
            JoinRightKey = RightKey.StartsWith(RightTable, StringComparison.OrdinalIgnoreCase) ? RightKey : RightTable + RightKey;
        }
    }
}
