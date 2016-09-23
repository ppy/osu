//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.UserInterface
{
    class KeyCounter : FlowContainer
    {
        public KeyCounter()
        {
            Direction = FlowDirection.HorizontalOnly;
        }

        public void AddKey(Count key) => base.Add(key);

        public override bool Contains(Vector2 screenSpacePos) => true;
    }
}
