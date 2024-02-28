// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mania.UI;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Skinning;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Mods
{
    public partial class ManiaModHidden : ManiaModWithPlayfieldCover, IApplicableToPlayer, IUpdatableByPlayfield
    {
        /// <summary>
        /// osu!stable is referenced to 768px.
        /// </summary>
        private const float reference_playfield_height = 768;

        public const float MIN_COVERAGE = 160f;
        public const float MAX_COVERAGE = 400f;
        private const float coverage_increase_per_combo = 0.5f;

        public override LocalisableString Description => @"Keys fade out before you hit them!";
        public override double ScoreMultiplier => 1;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[]
        {
            typeof(ManiaModFadeIn),
            typeof(ManiaModCover)
        }).ToArray();

        public override BindableNumber<float> Coverage { get; } = new BindableFloat(MIN_COVERAGE);
        protected override CoverExpandDirection ExpandDirection => CoverExpandDirection.AgainstScroll;

        private readonly IBindable<bool> isBreakTime = new Bindable<bool>();
        private readonly BindableInt combo = new BindableInt();

        public override void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            base.ApplyToScoreProcessor(scoreProcessor);

            combo.UnbindAll();
            combo.BindTo(scoreProcessor.Combo);
        }

        public void ApplyToPlayer(Player player)
        {
            isBreakTime.UnbindAll();
            isBreakTime.BindTo(player.IsBreakTime);
        }

        public void Update(Playfield playfield)
        {
            Coverage.Value = isBreakTime.Value
                ? 0
                : Math.Min(MAX_COVERAGE, MIN_COVERAGE + combo.Value * coverage_increase_per_combo) / reference_playfield_height;
        }

        protected override PlayfieldCoveringWrapper CreateCover(Drawable content) => new LegacyPlayfieldCover(content);

        private partial class LegacyPlayfieldCover : PlayfieldCoveringWrapper
        {
            [Resolved]
            private ISkinSource skin { get; set; } = null!;

            private IBindable<float>? hitPosition;

            public LegacyPlayfieldCover(Drawable content)
                : base(content)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                skin.SourceChanged += onSkinChanged;
                onSkinChanged();
            }

            private void onSkinChanged()
            {
                hitPosition = skin.GetManiaSkinConfig<float>(LegacyManiaSkinConfigurationLookups.HitPosition);
            }

            protected override float GetHeight(float coverage)
            {
                // In osu!stable, the cover is applied in absolute (x768) coordinates from the hit position.
                float availablePlayfieldHeight = Math.Abs(reference_playfield_height - (hitPosition?.Value ?? Stage.HIT_TARGET_POSITION));

                if (availablePlayfieldHeight == 0)
                    return base.GetHeight(coverage);

                return base.GetHeight(coverage) * reference_playfield_height / availablePlayfieldHeight;
            }
        }
    }
}
