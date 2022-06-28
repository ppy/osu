using System;

namespace osu.Game.Screens.LLin.Misc
{
    public class TypeWrapper
    {
        public Type Type { get; set; }
        public string Name { get; set; }

        public override string ToString() => Name;
    }
}
