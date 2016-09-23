using System.Collections.Generic;
﻿// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.KeyCounter
{
    /// <summary>
    /// Class that contains a series of keyboard/mouse button press counters
    /// Counters can be added using the AddKey method
    /// </summary>
    class KeyCounter : Drawable
    {
        class KeyCounterFlow : FlowContainer
        {
            public override bool Contains(Vector2 screenSpacePos) => true;
        }

        private KeyCounterFlow counterContainer;
        private List<Count> counterList;

        private bool isCounting = true;
        public bool IsCounting
        {
            get { return isCounting; }
            set
            {
                isCounting = value;

                foreach (Count counter in counterList)
                {
                    counter.isCounting = value;
                }
            }
        }

        public override bool Contains(Vector2 screenSpacePos) => true;

        public override void Load()
        {
            base.Load();

            counterList = new List<Count>();

            counterContainer = new KeyCounterFlow
            {
                Direction = FlowDirection.HorizontalOnly,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };

            Add(counterContainer);
        }

        internal void AddKey(Count counter)
        {
            counterContainer.Add(counter);
            counterList.Add(counter);
        }
    }
}