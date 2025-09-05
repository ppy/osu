// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;
using osu.Game.Localisation;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultRankDisplay : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        [SettingSource(typeof(DefaultRankDisplayStrings), nameof(DefaultRankDisplayStrings.PlaySamplesOnRankChange))]
        public BindableBool PlaySamples { get; set; } = new BindableBool(true);

        private UpdateableRank rankDisplay = null!;

        private SkinnableSound rankDownSample = null!;
        private SkinnableSound rankUpSample = null!;

        private Bindable<double?> lastSamplePlayback = null!;
        private double lastRankUpdate;

        private ScoreRank displayedRank;

        private const int minimum_update_rate = 3000;

        public DefaultRankDisplay()
        {
            Size = new Vector2(70, 35);
        }

        [BackgroundDependencyLoader]
        private void load(SkinEditor? skinEditor, SessionStatics statics)
        {
            InternalChildren = new Drawable[]
            {
                rankDownSample = new SkinnableSound(new SampleInfo("Gameplay/rank-down")),
                rankUpSample = new SkinnableSound(new SampleInfo("Gameplay/rank-up")),
                rankDisplay = new UpdateableRank(ScoreRank.X)
                {
                    RelativeSizeAxes = Axes.Both
                },
            };

            if (skinEditor != null)
                PlaySamples.Value = false;

            lastSamplePlayback = statics.GetBindable<double?>(Static.LastRankChangeSamplePlaybackTime);
        }

        protected override void Update()
        {
            base.Update();

            var currentRank = scoreProcessor.Rank.Value;

            if (currentRank != displayedRank)
            {
                bool enoughTimeElapsed = Time.Current - lastRankUpdate >= minimum_update_rate;

                if (enoughTimeElapsed || currentRank == ScoreRank.F)
                    updateRank(currentRank);
            }
        }

        private void updateRank(ScoreRank rank)
        {
            rankDisplay.Rank = rank;

            // Check sample time separately to ensure two copies of the rank display don't both play samples on a change.
            bool enoughSampleTimeElapsed = !lastSamplePlayback.Value.HasValue || Time.Current - lastSamplePlayback.Value >= OsuGameBase.SAMPLE_DEBOUNCE_TIME;

            // Also don't play rank-down sfx on quit/retry/initial update.
            if (rank != displayedRank && rank > ScoreRank.F && PlaySamples.Value && enoughSampleTimeElapsed && lastRankUpdate > 0)
            {
                if (rank > displayedRank)
                    rankUpSample.Play();
                else
                    rankDownSample.Play();

                lastSamplePlayback.Value = Time.Current;
            }

            displayedRank = rank;
            lastRankUpdate = Time.Current;
        }
    }
}
