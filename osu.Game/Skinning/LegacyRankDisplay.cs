// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Skinning
{
    public partial class LegacyRankDisplay : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        [Resolved]
        private ISkinSource source { get; set; } = null!;

        [SettingSource(typeof(DefaultRankDisplayStrings), nameof(DefaultRankDisplayStrings.PlaySamplesOnRankChange))]
        public BindableBool PlaySamples { get; set; } = new BindableBool(true);

        private readonly Sprite rankDisplay;

        private SkinnableSound rankDownSample = null!;
        private SkinnableSound rankUpSample = null!;

        private Bindable<double?> lastSamplePlayback = null!;
        private double lastChangeTime;

        private ScoreRank? displayedRank;

        private const int time_between_changes = 1500;

        public LegacyRankDisplay()
        {
            AutoSizeAxes = Axes.Both;

            AddInternal(rankDisplay = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        [BackgroundDependencyLoader]
        private void load(SkinEditor? skinEditor, SessionStatics statics)
        {
            AddRangeInternal(new Drawable[]
            {
                rankDownSample = new SkinnableSound(new SampleInfo("Gameplay/rank-down")),
                rankUpSample = new SkinnableSound(new SampleInfo("Gameplay/rank-up")),
            });

            if (skinEditor != null)
                PlaySamples.Value = false;

            lastSamplePlayback = statics.GetBindable<double?>(Static.LastRankChangeSamplePlaybackTime);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateRank(scoreProcessor.Rank.Value);
        }

        protected override void Update()
        {
            base.Update();

            var currentRank = scoreProcessor.Rank.Value;

            if (currentRank == displayedRank)
                return;

            if (Time.Current - lastChangeTime >= time_between_changes || scoreProcessor.HasCompleted.Value || currentRank == ScoreRank.F)
                updateRank(currentRank);
        }

        private void updateRank(ScoreRank rank)
        {
            var texture = source.GetTexture($"ranking-{rank}-small");

            rankDisplay.Texture = texture;

            if (texture != null && displayedRank != null)
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

            // Check sample time separately to ensure two copies of the rank display don't both play samples on a change.
            bool enoughSampleTimeElapsed = !lastSamplePlayback.Value.HasValue || Time.Current - lastSamplePlayback.Value >= OsuGameBase.SAMPLE_DEBOUNCE_TIME;

            // Also don't play rank-down sfx on quit/retry/initial update.
            if (displayedRank != null && rank > ScoreRank.F && PlaySamples.Value && enoughSampleTimeElapsed)
            {
                if (rank > displayedRank)
                    rankUpSample.Play();
                else
                    rankDownSample.Play();

                lastSamplePlayback.Value = Time.Current;
            }

            displayedRank = rank;
            lastChangeTime = Time.Current;
        }
    }
}
