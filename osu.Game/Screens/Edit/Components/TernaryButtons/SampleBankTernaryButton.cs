// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Screens.Edit.Components.TernaryButtons
{
    public partial class SampleBankTernaryButton : CompositeDrawable
    {
        public string BankName { get; }
        public required Func<Drawable> CreateIcon { get; init; }

        public required Func<Drawable> CreateCompactIcon { get; init; }

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
                        CreateCompactIcon = CreateCompactIcon,
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
                        CreateCompactIcon = CreateCompactIcon,
                    },
                },
            };
        }

        private partial class InlineDrawableTernaryButton : DrawableTernaryButton, IExpandable
        {
            public new required Func<Drawable> CreateIcon { get; init; }

            public required Func<Drawable> CreateCompactIcon { get; init; }

            private Drawable icon = null!;
            private Drawable iconCompact = null!;

            public BindableBool Expanded { get; } = new BindableBool();

            [Resolved(canBeNull: true)]
            private IExpandingContainer? expandingContainer { get; set; }

            public InlineDrawableTernaryButton()
            {
                base.CreateIcon = createBaseIcon;
            }

            private Drawable createBaseIcon() => new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new[]
                {
                    icon = CreateIcon(),
                    iconCompact = CreateCompactIcon(),
                }
            };

            [BackgroundDependencyLoader]
            private void load()
            {
                Content.Masking = false;
                Content.CornerRadius = 0;
                Icon.X = 4.5f;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                expandingContainer?.Expanded.BindValueChanged(containerExpanded =>
                {
                    Expanded.Value = containerExpanded.NewValue;
                }, true);

                Expanded.BindValueChanged(expanded =>
                {
                    icon.FadeTo(expanded.NewValue ? 1 : 0, 150, Easing.OutQuint);
                    iconCompact.FadeTo(expanded.NewValue ? 0 : 1, 150, Easing.OutQuint);
                }, true);
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
