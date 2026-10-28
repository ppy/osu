// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.RankingV2.Legacy
{
    /// <seealso href="https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L570-L608"/>
    public partial class LegacyRankingGrade : CompositeDrawable, ISerialisableDrawable, IAnimatableSkinnable
    {
        private Sprite gradeSprite = null!;
        private Sprite gradeAdditiveSprite = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        private ScoreRank? lastShownRank;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren =
            [
                gradeSprite = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                gradeAdditiveSprite = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            var rank = score.Value.Rank;

            if (lastShownRank == rank)
                return;

            // The fixed size is transferred manually from the sprite instead of using `AutoSizeAxes = Axes.Both`
            // so that the bounding box of this component does not change as the animation of this component plays.
            gradeAdditiveSprite.Size = gradeSprite.Size = Vector2.Zero;
            gradeAdditiveSprite.Texture = gradeSprite.Texture = skin.GetTexture($@"ranking-{rank.ToString()}");
            Size = gradeSprite.Size;
            lastShownRank = rank;
        }

        #region ISerialisableDrawable

        public bool UsesFixedAnchor { get; set; }

        #endregion

        #region IAnimatableSkinnable

        [SettingSource(nameof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.AnimationSequence))]
        public BindableInt GroupNumber { get; } = new BindableInt(1)
        {
            MinValue = 0,
            MaxValue = 10,
        };

        int IAnimatableSkinnable.GroupNumber => GroupNumber.Value;

        public double StartAnimating(double startTime)
        {
            FinishAnimating();

            gradeSprite.ScaleTo(new Vector2(2));
            gradeSprite.FadeOut();
            gradeAdditiveSprite.ScaleTo(Vector2.One);
            gradeAdditiveSprite.FadeOut();

            const double basic_transition_duration = 1000;
            const double flash_duration = 3400 - 1000;

            using (BeginAbsoluteSequence(startTime))
            {
                // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L572-L586
                gradeSprite.ScaleTo(Vector2.One, basic_transition_duration, Easing.In);
                gradeSprite.FadeIn(basic_transition_duration, Easing.In);

                if (score.Value.Rank > ScoreRank.C)
                {
                    // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L588-L607
                    using (BeginDelayedSequence(basic_transition_duration))
                    {
                        gradeAdditiveSprite.ScaleTo(new Vector2(1.05f), flash_duration, Easing.Out);
                        gradeAdditiveSprite.FadeOutFromOne(flash_duration, Easing.Out);
                    }
                }
            }

            return Math.Max(gradeSprite.LatestTransformEndTime, gradeAdditiveSprite.LatestTransformEndTime);
        }

        public void FinishAnimating()
        {
            gradeSprite.FinishTransforms();
            gradeAdditiveSprite.FinishTransforms();
        }

        #endregion
    }
}
