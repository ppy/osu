// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Bindables;
using System;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Framework.Localisation;

namespace osu.Game.Overlays.Profile.Sections
{
    public partial class ProfileSubsectionHeader : CompositeDrawable, IHasCurrentValue<int>
    {
        private readonly BindableWithCurrent<int> current = new BindableWithCurrent<int>();

        public Bindable<int> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly LocalisableString text;
        private readonly CounterVisibilityState counterState;

        private CounterPill counterPill = null!;

        public ProfileSubsectionHeader(LocalisableString text, CounterVisibilityState counterState)
        {
            this.text = text;
            this.counterState = counterState;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AutoSizeAxes = Axes.Both;
            Padding = new MarginPadding { Vertical = 10 };
            InternalChildren = new Drawable[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Height = 0.65f,
                    Width = 3,
                    Masking = true,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding { Right = 10 },
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Highlight1
                    }
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10, 0),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = text,
                            Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                        },
                        counterPill = new CounterPill
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Current = { BindTarget = current }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            current.BindValueChanged(onCurrentChanged, true);
        }

        private void onCurrentChanged(ValueChangedEvent<int> countValue)
        {
            float alpha;

            switch (counterState)
            {
                case CounterVisibilityState.AlwaysHidden:
                    alpha = 0;
                    break;

                case CounterVisibilityState.AlwaysVisible:
                    alpha = 1;
                    break;

                case CounterVisibilityState.VisibleWhenZero:
                    alpha = current.Value == 0 ? 1 : 0;
                    break;

                default:
                    throw new NotImplementedException($"{counterState} has an incorrect value.");
            }

            counterPill.Alpha = alpha;
        }
    }

    public enum CounterVisibilityState
    {
        AlwaysHidden,
        AlwaysVisible,
        VisibleWhenZero
    }
}
