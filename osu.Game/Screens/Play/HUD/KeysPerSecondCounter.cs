// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class KeysPerSecondCounter : RollingCounter<int>, ISkinnableDrawable
    {
        private static List<double> timestamps;
        private static double maxTime = double.NegativeInfinity;

        private static event Action onNewInput;

        private const int invalidation_timeout = 1000;
        private const float alpha_when_invalid = 0.3f;

        private readonly Bindable<bool> valid = new Bindable<bool>();

        private static GameplayClock gameplayClock;
        private static IClock referenceClock;

        private static IClock clock => referenceClock ?? gameplayClock;

        [Resolved(canBeNull: true)]
        private DrawableRuleset drawableRuleset { get; set; }

        [SettingSource("Smoothing time", "How smooth the counter should change\nThe more it is smooth, the less it's accurate.")]
        public BindableNumber<double> SmoothingTime { get; } = new BindableNumber<double>(350)
        {
            MaxValue = 1000,
            MinValue = 0
        };

        public static void AddTimestamp()
        {
            Logger.Log($"Input timestamp attempt C: {clock.CurrentTime}ms | GC: {gameplayClock.CurrentTime} | RC: {referenceClock?.CurrentTime ?? -1} | Max: {maxTime})", level: LogLevel.Debug);

            if (clock.CurrentTime >= maxTime)
            {
                Logger.Log("Input timestamp added.", level: LogLevel.Debug);
                timestamps?.Add(clock.CurrentTime);
                maxTime = timestamps?.Max() ?? clock.CurrentTime;
            }

            onNewInput?.Invoke();
        }

        public static void Reset()
        {
            timestamps?.Clear();
            maxTime = int.MinValue;
        }

        protected override double RollingDuration => SmoothingTime.Value;

        public bool UsesFixedAnchor { get; set; }

        public KeysPerSecondCounter()
        {
            timestamps ??= new List<double>();
            Current.Value = 0;
            onNewInput += updateCounter;
            Scheduler.AddOnce(updateCounter);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, GameplayClock clock)
        {
            gameplayClock = clock;
            Colour = colours.BlueLighter;
            valid.BindValueChanged(e =>
                DrawableCount.FadeTo(e.NewValue ? 1 : alpha_when_invalid, 1000, Easing.OutQuint));
            referenceClock = drawableRuleset?.FrameStableClock;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateCounter();
        }

        protected override void Update()
        {
            base.Update();

            updateCounter();
        }

        private void updateCounter()
        {
            valid.Value = timestamps != null && MathHelper.ApproximatelyEquivalent(gameplayClock.CurrentTime, referenceClock.CurrentTime, 500);
            Current.Value = timestamps?.Count(timestamp => clock.CurrentTime - timestamp is >= 0 and <= invalidation_timeout) ?? 0;
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
