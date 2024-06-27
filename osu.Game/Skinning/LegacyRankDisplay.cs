// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning
{
    public partial class LegacyRankDisplay : CompositeDrawable, ISerialisableDrawable
    {
        [SettingSource(typeof(Localisation.HUD.GameplayRankDisplayStrings), nameof(Localisation.HUD.GameplayRankDisplayStrings.RankDisplay), nameof(Localisation.HUD.GameplayRankDisplayStrings.RankDisplayDescription))]
        public Bindable<DefaultRankDisplay.RankDisplayMode> RankDisplay { get; } = new Bindable<DefaultRankDisplay.RankDisplayMode>();

        public bool UsesFixedAnchor { get; set; }

        private Bindable<Scoring.ScoreRank> rankBinding = new Bindable<Scoring.ScoreRank>();

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        [Resolved]
        private ISkinSource source { get; set; } = null!;

        private readonly Sprite rank;

        public LegacyRankDisplay()
        {
            AutoSizeAxes = Axes.Both;

            AddInternal(rank = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            RankDisplay.BindValueChanged(mode =>
            {
                rankBinding.UnbindBindings();
                switch (mode.NewValue)
                {
                    case DefaultRankDisplay.RankDisplayMode.Standard:
                        rankBinding.BindTarget = scoreProcessor.Rank;
                        break;

                    case DefaultRankDisplay.RankDisplayMode.MinimumAchievable:
                        rankBinding.BindTarget = scoreProcessor.MinimumRank;
                        break;

                    case DefaultRankDisplay.RankDisplayMode.MaximumAchievable:
                        rankBinding.BindTarget = scoreProcessor.MaximumRank;
                        break;
                }
                rankBinding.BindValueChanged(v => updateValue(v), true);
            }, true);
            FinishTransforms(true);
        }

        private void updateValue(ValueChangedEvent<Scoring.ScoreRank> v)
        {
            var texture = source.GetTexture($"ranking-{v.NewValue}-small");

            rank.Texture = texture;

            if (texture != null)
            {
                var transientRank = new Sprite
                {
                    Texture = texture,
                    Blending = BlendingParameters.Additive,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BypassAutoSizeAxes = Axes.Both,
                };
                AddInternal(transientRank);
                transientRank.FadeOutFromOne(500, Easing.Out)
                             .ScaleTo(new Vector2(1.625f), 500, Easing.Out)
                             .Expire();
            }
        }
    }
}
