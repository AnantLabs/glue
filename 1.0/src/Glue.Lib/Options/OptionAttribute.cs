using System;

namespace Glue.Lib.Options
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class OptionAttribute : Attribute
    {
        public string Name;
        public char   Short;
        public string Help;
        public int MinOccurs;
        public int MaxOccurs; // negative means there is no limit

        public OptionAttribute()
        {
            MinOccurs = 0;
            MaxOccurs = 1;
        }
    }

    public class RequiredOptionAttribute : OptionAttribute
    {
        public RequiredOptionAttribute()
        {
            MinOccurs = 1;
            MaxOccurs = 1;
        }
    }

    public class AnyOptionAttribute : OptionAttribute
    {
        public AnyOptionAttribute()
        {
            MinOccurs = 0;
            MaxOccurs = -1;
        }
    }

    public class AnonymousOptionAttribute : OptionAttribute
    {
        public AnonymousOptionAttribute()
        {
            MinOccurs = 0;
            MaxOccurs = -1;
        }
    }
}
