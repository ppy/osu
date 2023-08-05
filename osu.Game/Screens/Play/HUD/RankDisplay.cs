// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Extensions;

namespace osu.Game.Screens.Play.HUD
{
    public partial class RankDisplay : FontAdjustableSkinComponent
    {
        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        private readonly OsuSpriteText text;

        public RankDisplay()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            text.Text = scoreProcessor.Rank.Value.GetDescription();

            scoreProcessor.Rank.BindValueChanged(v => text.Text = v.NewValue.GetDescription());
        }

        protected override void SetFont(FontUsage font) => text.Font = font.With(size: 40);
    }
}
