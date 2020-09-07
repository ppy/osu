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

namespace osu.Game.Overlays.Profile.Sections
{
    public class PaginatedContainerHeader : CompositeDrawable, IHasCurrentValue<int>
    {
        public Bindable<int> Current
        {
            get => current;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                current.UnbindBindings();
                current.BindTo(value);
            }
        }

        private readonly Bindable<int> current = new Bindable<int>();

        private readonly string text;
        private readonly CounterVisibilityState counterState;

        private CounterPill counterPill;

        public PaginatedContainerHeader(string text, CounterVisibilityState counterState)
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
                            Alpha = getInitialCounterAlpha(),
                            Current = { BindTarget = current }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            current.BindValueChanged(onCurrentChanged);
        }

        private float getInitialCounterAlpha()
        {
            switch (counterState)
            {
                case CounterVisibilityState.AlwaysHidden:
                    return 0;

                case CounterVisibilityState.AlwaysVisible:
                    return 1;

                case CounterVisibilityState.VisibleWhenZero:
                    return current.Value == 0 ? 1 : 0;

                default:
                    throw new NotImplementedException($"{counterState} has an incorrect value.");
            }
        }

        private void onCurrentChanged(ValueChangedEvent<int> countValue)
        {
            if (counterState == CounterVisibilityState.VisibleWhenZero)
            {
                counterPill.Alpha = countValue.NewValue == 0 ? 1 : 0;
            }
        }
    }

    public enum CounterVisibilityState
    {
        AlwaysHidden,
        AlwaysVisible,
        VisibleWhenZero
    }
}
