//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.UserInterface
{
    public class KeyCounterCollection : FlowContainer
    {
        public KeyCounterCollection()
        {
            Direction = FlowDirection.HorizontalOnly;
        }

        public void AddKey(KeyCounter key)
        {
            key.ParentCounter = this;
            base.Add(key);
        }

        public override bool Contains(Vector2 screenSpacePos) => true;

        public bool IsCounting { get; set; }
    }
}
