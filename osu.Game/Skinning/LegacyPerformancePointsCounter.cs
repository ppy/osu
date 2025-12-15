// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning
{
    public partial class LegacyPerformancePointsCounter : PerformancePointsCounter, ISerialisableDrawable
    {
        protected override double RollingDuration => 1000;
        protected override Easing RollingEasing => Easing.Out;

        private const float alpha_when_invalid = 0.3f;

        public LegacyPerformancePointsCounter()
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;

            Scale = new Vector2(0.96f);
        }

        public override bool IsValid
        {
            get => base.IsValid;
            set
            {
                if (value == IsValid)
                    return;

                base.IsValid = value;
                DrawableCount.FadeTo(value ? 1 : alpha_when_invalid, 1000, Easing.OutQuint);
            }
        }

        protected override LocalisableString FormatCount(int count) => count.ToString($@"0'{LegacySpriteText.PP_SUFFIX_CHAR}'");

        protected sealed override OsuSpriteText CreateSpriteText() => new LegacySpriteText(LegacyFont.Score);
    }
}
