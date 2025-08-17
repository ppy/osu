// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Screens.Edit.Components.TernaryButtons
{
    public partial class SampleBankTernaryButton : CompositeDrawable
    {
        public string BankName { get; }
        public Func<Drawable>? CreateIcon { get; init; }

        public readonly BindableWithCurrent<TernaryState> NormalState = new BindableWithCurrent<TernaryState>();
        public readonly BindableWithCurrent<TernaryState> AdditionsState = new BindableWithCurrent<TernaryState>();

        public DrawableTernaryButton NormalButton { get; private set; } = null!;
        public DrawableTernaryButton AdditionsButton { get; private set; } = null!;

        public SampleBankTernaryButton(string bankName)
        {
            BankName = bankName;
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
                    Child = NormalButton = new InlineDrawableTernaryButton
                    {
                        Current = NormalState,
                        Description = BankName.Titleize(),
                        CreateIcon = CreateIcon,
                    },
                },
                new Container
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.5f,
                    Padding = new MarginPadding { Left = 1 },
                    Child = AdditionsButton = new InlineDrawableTernaryButton
                    {
                        Current = AdditionsState,
                        Description = BankName.Titleize(),
                        CreateIcon = CreateIcon,
                    },
                },
            };
        }

        private partial class InlineDrawableTernaryButton : DrawableTernaryButton
        {
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
}
