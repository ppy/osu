// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Mods
{
    public sealed partial class DifficultyMultiplierDisplay : ModsEffectDisplay
    {
        protected override LocalisableString Label => DifficultyMultiplierDisplayStrings.DifficultyMultiplier;

        protected override string CounterFormat => @"N2";

        public DifficultyMultiplierDisplay()
        {
            Current.Default = 1d;
            Current.Value = 1d;
            Add(new SpriteIcon
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Icon = FontAwesome.Solid.Times,
                Size = new Vector2(7),
                Margin = new MarginPadding { Top = 1 }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // required to prevent the counter initially rolling up from 0 to 1
            // due to `Current.Value` having a nonstandard default value of 1.
            Counter.SetCountWithoutRolling(Current.Value);
        }
    }
}
