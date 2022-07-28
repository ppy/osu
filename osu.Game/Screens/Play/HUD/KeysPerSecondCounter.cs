// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class KeysPerSecondCounter : RollingCounter<int>, ISkinnableDrawable
    {
        private static Queue<DateTime>? timestamps;

        private static event Action? onNewInput;
        private readonly TimeSpan refreshSpan = TimeSpan.FromSeconds(1);

        private const float alpha_when_invalid = 0.3f;
        private readonly Bindable<bool> valid = new Bindable<bool>();

        public static void AddTimestamp()
        {
            timestamps?.Enqueue(DateTime.Now);
            onNewInput?.Invoke();
        }

        protected override double RollingDuration => 250;

        public bool UsesFixedAnchor { get; set; }

        public KeysPerSecondCounter()
        {
            timestamps ??= new Queue<DateTime>();
            Current.Value = 0;
            onNewInput += updateCounter;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueLighter;
            valid.BindValueChanged(e =>
                DrawableCount.FadeTo(e.NewValue ? 1 : alpha_when_invalid, 1000, Easing.OutQuint));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateCounter();
        }

        protected override void Update()
        {
            if (timestamps != null)
            {
                if (timestamps.TryPeek(out var earliest) && DateTime.Now - earliest >= refreshSpan)
                    timestamps.Dequeue();
            }

            updateCounter();

            base.Update();
        }

        private void updateCounter()
        {
            valid.Value = timestamps != null;
            Current.Value = timestamps?.Count ?? 0;
        }

        protected override IHasText CreateText() => new TextComponent
        {
            Alpha = alpha_when_invalid
        };

        private class TextComponent : CompositeDrawable, IHasText
        {
            public LocalisableString Text
            {
                get => text.Text;
                set => text.Text = value;
            }

            private readonly OsuSpriteText text;

            public TextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(2),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 16, fixedWidth: true)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 8, fixedWidth: true),
                            Text = @"KPS",
                            Padding = new MarginPadding { Bottom = 1.5f }, // align baseline better
                        }
                    }
                };
            }
        }
    }
}
