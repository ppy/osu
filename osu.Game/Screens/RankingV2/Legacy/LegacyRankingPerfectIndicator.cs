// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.RankingV2.Legacy
{
    /// <seealso href="https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L445-L451"/>
    public partial class LegacyRankingPerfectIndicator : CompositeDrawable, ISerialisableDrawable, IAnimatableSkinnable
    {
        private Sprite perfectIndicator = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            InternalChild = perfectIndicator = new Sprite
            {
                Texture = skin.GetTexture(@"ranking-perfect"),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            Size = perfectIndicator.Size;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            perfectIndicator.Alpha = score.Value.IsPerfect() ? 1 : 0;
        }

        #region ISerialisableDrawable

        public bool UsesFixedAnchor { get; set; }

        // TODO: temporary to avoid `SkinDeserialisationTest` failures.
        // Remove when the legacy skin supplies a default layout of results with this component included.
        public bool IsEditable => false;

        #endregion

        #region IAnimatableSkinnable

        [SettingSource(nameof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.AnimationSequence))]
        public BindableInt GroupNumber { get; } = new BindableInt
        {
            MinValue = 0,
            MaxValue = 10,
        };

        int IAnimatableSkinnable.GroupNumber => GroupNumber.Value;

        public double StartAnimating(double startTime)
        {
            FinishAnimating();

            // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L448
            perfectIndicator
                .ScaleTo(1.1f)
                .FadeOut();

            if (score.Value.IsPerfect())
            {
                using (BeginAbsoluteSequence(startTime))
                {
                    perfectIndicator.ScaleTo(1f, 300, Easing.Out)
                                    .FadeIn(300, Easing.Out);
                }
            }

            return perfectIndicator.LatestTransformEndTime;
        }

        public void FinishAnimating()
        {
            perfectIndicator.FinishTransforms();
        }

        #endregion
    }
}
