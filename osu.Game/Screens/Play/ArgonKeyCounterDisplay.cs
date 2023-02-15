// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Play
{
    public partial class ArgonKeyCounterDisplay : KeyCounterDisplay
    {
        private const int duration = 100;

        public new IReadOnlyList<ArgonKeyCounter> Children
        {
            get => (IReadOnlyList<ArgonKeyCounter>)base.Children;
            set => base.Children = value;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            KeyFlow.Direction = FillDirection.Horizontal;
            KeyFlow.AutoSizeAxes = Axes.Both;
            KeyFlow.Spacing = new Vector2(2);

            InternalChildren = new[]
            {
                KeyFlow
            };
        }

        protected override bool CheckType(KeyCounter key) => key is ArgonKeyCounter;

        protected override void UpdateVisibility()
            => KeyFlow.FadeTo(AlwaysVisible.Value || ConfigVisibility.Value ? 1 : 0, duration);

        public override KeyCounter CreateKeyCounter(KeyCounter.InputTrigger trigger) => new ArgonKeyCounter(trigger);
    }
}
