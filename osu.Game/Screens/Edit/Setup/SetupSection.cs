// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Screens.Edit.Setup
{
    internal class SetupSection : Container
    {
        private readonly FillFlowContainer flow;

        [Resolved]
        protected OsuColour Colours { get; private set; }

        [Resolved]
        protected EditorBeatmap Beatmap { get; private set; }

        protected override Container<Drawable> Content => flow;

        public SetupSection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding(10);

            InternalChild = flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(20),
                Direction = FillDirection.Vertical,
            };
        }
    }
}
