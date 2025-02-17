// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.UI
{
    public partial class DrawableManiaJudgement : DrawableJudgement
    {
        private IBindable<ScrollingDirection> direction;

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            direction = scrollingInfo.Direction.GetBoundCopy();
            direction.BindValueChanged(_ => onDirectionChanged(), true);
        }

        private void onDirectionChanged()
        {
            Anchor = direction.Value == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
            Origin = Anchor.Centre;
        }

        protected override Drawable CreateDefaultJudgement(HitResult result) => new DefaultManiaJudgementPiece(result);
    }
}
