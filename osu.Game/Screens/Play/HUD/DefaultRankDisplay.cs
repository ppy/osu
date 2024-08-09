// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultRankDisplay : GameplayRankDisplay, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        private readonly UpdateableRank drawableRank;

        public DefaultRankDisplay()
        {
            Size = new Vector2(70, 35);

            AddInternal(drawableRank = new UpdateableRank(ScoreRank.X)
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Current.BindValueChanged(v => drawableRank.Rank = v.NewValue, true);
        }
    }
}
