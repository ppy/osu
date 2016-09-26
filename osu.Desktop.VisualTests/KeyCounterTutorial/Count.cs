using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;

namespace osu.Desktop.KeyCounterTutorial
{
    internal abstract class Count : Container
    {
        public bool IsCounting { get; set; }
        public bool IsLit { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }

        public Count(string name)
        {
            Name = name;
        }
    }
}