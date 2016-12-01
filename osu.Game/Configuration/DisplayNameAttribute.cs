using System;
namespace osu.Game.Configuration
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DisplayNameAttribute : Attribute
    {
        public string Name { get; set; }
    
        public DisplayNameAttribute(string name)
        {
            Name = name;
        }
    }
}

