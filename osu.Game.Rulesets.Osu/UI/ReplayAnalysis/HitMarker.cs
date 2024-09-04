// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Pooling;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class HitMarker : PoolableDrawableWithLifetime<AnalysisFrameEntry>
    {
        public HitMarker()
        {
            Origin = Anchor.Centre;
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        protected override void OnApply(AnalysisFrameEntry entry)
        {
            Position = entry.Position;

            using (BeginAbsoluteSequence(LifetimeStart))
                Show();

            using (BeginAbsoluteSequence(LifetimeEnd - 200))
                this.FadeOut(200);

            switch (entry.Action)
            {
                case OsuAction.LeftButton:
                    Colour = colours.BlueLight;
                    break;

                case OsuAction.RightButton:
                    Colour = colours.YellowLight;
                    break;

                default:
                    Colour = colours.Pink2;
                    break;
            }
        }
    }
}
