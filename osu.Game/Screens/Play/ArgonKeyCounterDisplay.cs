// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Screens.Play
{
    public partial class ArgonKeyCounterDisplay : KeyCounterDisplay
    {
        private const int duration = 100;

        private readonly FillFlowContainer<ArgonKeyCounter> keyFlow;

        public override IEnumerable<KeyCounter> Counters => keyFlow;

        public ArgonKeyCounterDisplay()
        {
            InternalChild = keyFlow = new FillFlowContainer<ArgonKeyCounter>
            {
                Direction = FillDirection.Horizontal,
                AutoSizeAxes = Axes.Both,
                Alpha = 0,
                Spacing = new Vector2(2),
            };
        }

        protected override void Update()
        {
            base.Update();

            Size = keyFlow.Size;
        }

        public override void Add(InputTrigger trigger) =>
            keyFlow.Add(new ArgonKeyCounter(trigger));

        protected override void UpdateVisibility()
            => keyFlow.FadeTo(AlwaysVisible.Value || ConfigVisibility.Value ? 1 : 0, duration);
    }
}
