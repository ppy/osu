// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning
{
    public class LegacyScoreCounter : GameplayScoreCounter, ISkinnableComponent
    {
        private readonly ISkin skin;

        protected override double RollingDuration => 1000;
        protected override Easing RollingEasing => Easing.Out;

        public LegacyScoreCounter(ISkin skin)
            : base(6)
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;

            this.skin = skin;

            Scale = new Vector2(0.96f);
            Margin = new MarginPadding(10);
        }

        protected sealed override OsuSpriteText CreateSpriteText() => new LegacySpriteText(skin, LegacyFont.Score)
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
        };
    }
}
