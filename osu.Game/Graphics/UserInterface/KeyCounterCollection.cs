//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
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

        private List<KeyCounter> counters = new List<KeyCounter>();
        //default capacity is 4, and osu! uses 4 keys usually, so it won't trouble
        public IReadOnlyList<KeyCounter> Counters => counters;

        public void AddKey(KeyCounter key)
        {
            counters.Add(key);
            key.IsCounting = this.IsCounting;
            base.Add(key);
        }

        public override bool Contains(Vector2 screenSpacePos) => true;

        private bool isCounting;
        public bool IsCounting
        {
            get { return isCounting; }
            set
            {
                isCounting = value;
                foreach (var child in counters)
                    child.IsCounting = value;
            }
        }
    }
}
