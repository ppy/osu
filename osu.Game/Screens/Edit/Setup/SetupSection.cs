// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Edit.Setup
{
    public abstract partial class SetupSection : Container
    {
        private FillFlowContainer flow = null!;

        /// <summary>
        /// Used to align some of the child <see cref="LabelledDrawable{T}"/>s together to achieve a grid-like look.
        /// </summary>
        protected const float LABEL_WIDTH = 160;

        [Resolved]
        protected OsuColour Colours { get; private set; } = null!;

        [Resolved]
        protected EditorBeatmap Beatmap { get; private set; } = null!;

        protected override Container<Drawable> Content => flow;

        public abstract LocalisableString Title { get; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding
            {
                Vertical = 10,
                Horizontal = 100
            };

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(20),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(weight: FontWeight.Bold),
                        Text = Title
                    },
                    flow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(10),
                        Direction = FillDirection.Vertical,
                    }
                }
            };
        }
    }
}
