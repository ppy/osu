// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning
{
    public partial class LegacyScoreCounter : GameplayScoreCounter, ISerialisableDrawable
    {
        protected override double RollingDuration => 1000;
        protected override Easing RollingEasing => Easing.Out;

        public bool UsesFixedAnchor { get; set; }

        public LegacyScoreCounter()
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;

            Scale = new Vector2(0.96f);
            Margin = new MarginPadding { Horizontal = 10 };
        }

        protected sealed override OsuSpriteText CreateSpriteText() => new LegacySpriteText(LegacyFont.Score)
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            FixedWidth = true,
        };
    }
}
