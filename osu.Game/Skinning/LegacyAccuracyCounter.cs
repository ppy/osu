// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning
{
    public class LegacyAccuracyCounter : PercentageCounter, IAccuracyCounter
    {
        private readonly ISkin skin;

        private readonly string scorePrefix;
        private readonly int scoreOverlap;

        public LegacyAccuracyCounter(ISkin skin)
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;

            Scale = new Vector2(0.75f);
            Margin = new MarginPadding(10);

            this.skin = skin;
            scorePrefix = skin.GetConfig<LegacySkinConfiguration.LegacySetting, string>(LegacySkinConfiguration.LegacySetting.ScorePrefix)?.Value ?? "score";
            scoreOverlap = skin.GetConfig<LegacySkinConfiguration.LegacySetting, int>(LegacySkinConfiguration.LegacySetting.ScoreOverlap)?.Value ?? -2;
        }

        [Resolved(canBeNull: true)]
        private HUDOverlay hud { get; set; }

        protected sealed override OsuSpriteText CreateSpriteText() =>
            new LegacySpriteText(skin, scorePrefix)
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Spacing = new Vector2(-scoreOverlap, 0)
            };

        protected override void Update()
        {
            base.Update();

            if (hud?.ScoreCounter.Drawable is LegacyScoreCounter score)
            {
                // for now align with the score counter. eventually this will be user customisable.
                Y = Parent.ToLocalSpace(score.ScreenSpaceDrawQuad.BottomRight).Y;
            }
        }
    }
}
