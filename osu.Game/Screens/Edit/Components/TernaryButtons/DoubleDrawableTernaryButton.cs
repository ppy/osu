// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Screens.Edit.Components.TernaryButtons
{
    public partial class DoubleDrawableTernaryButton : CompositeDrawable
    {
        public readonly TernaryButton Button1;
        public readonly TernaryButton Button2;

        public DoubleDrawableTernaryButton(TernaryButton button1, TernaryButton button2)
        {
            Button1 = button1;
            Button2 = button2;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.5f,
                    Padding = new MarginPadding { Right = 1 },
                    Child = new InlineDrawableTernaryButton(Button1),
                },
                new Container
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.5f,
                    Padding = new MarginPadding { Left = 1 },
                    Child = new InlineDrawableTernaryButton(Button2),
                },
            };
        }
    }

    public partial class InlineDrawableTernaryButton : DrawableTernaryButton
    {
        public InlineDrawableTernaryButton(TernaryButton button)
            : base(button)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.Masking = false;
            Content.CornerRadius = 0;
            Icon.X = 4.5f;
        }

        protected override SpriteText CreateText() => new ExpandableSpriteText
        {
            Depth = -1,
            Origin = Anchor.CentreLeft,
            Anchor = Anchor.CentreLeft,
            X = 31f
        };
    }
}
