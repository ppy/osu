// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Mods
{
    public sealed class DifficultyMultiplierDisplay : ModsEffectDisplay
    {
        protected override LocalisableString Label => DifficultyMultiplierDisplayStrings.DifficultyMultiplier;

        private readonly MultiplierCounter multiplierCounter;

        public DifficultyMultiplierDisplay()
        {
            Current.Default = 1d;
            Current.Value = 1d;
            Add(new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(2, 0),
                Children = new Drawable[]
                {
                    multiplierCounter = new MultiplierCounter
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Current = { BindTarget = Current }
                    },
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = FontAwesome.Solid.Times,
                        Size = new Vector2(7),
                        Margin = new MarginPadding { Top = 1 }
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // required to prevent the counter initially rolling up from 0 to 1
            // due to `Current.Value` having a nonstandard default value of 1.
            multiplierCounter.SetCountWithoutRolling(Current.Value);
        }

        private class MultiplierCounter : RollingCounter<double>
        {
            protected override double RollingDuration => 500;

            protected override LocalisableString FormatCount(double count) => count.ToLocalisableString(@"N2");

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Font = OsuFont.Default.With(size: 17, weight: FontWeight.SemiBold)
            };
        }
    }
}
