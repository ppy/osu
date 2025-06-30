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
        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        [SettingSource(typeof(DefaultRankDisplayStrings), nameof(DefaultRankDisplayStrings.PlaySamplesOnRankChange))]
        public BindableBool PlaySamples { get; set; } = new BindableBool(true);

        public bool UsesFixedAnchor { get; set; }

        private UpdateableRank rankDisplay = null!;

        private SkinnableSound rankDownSample = null!;
        private SkinnableSound rankUpSample = null!;

        private Bindable<double?> lastSamplePlaybackTime = null!;

        private IBindable<ScoreRank> rank = null!;

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

            lastSamplePlaybackTime = statics.GetBindable<double?>(Static.LastRankChangeSamplePlaybackTime);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            rank = scoreProcessor.Rank.GetBoundCopy();
            rank.BindValueChanged(r =>
            {
                bool enoughTimeElapsed = !lastSamplePlaybackTime.Value.HasValue || Time.Current - lastSamplePlaybackTime.Value >= OsuGameBase.SAMPLE_DEBOUNCE_TIME;

                // Don't play rank-down sfx on quit/retry
                if (r.NewValue != r.OldValue && r.NewValue > ScoreRank.F && PlaySamples.Value && enoughTimeElapsed)
                {
                    if (r.NewValue > rankDisplay.Rank)
                        rankUpSample.Play();
                    else
                        rankDownSample.Play();

                    lastSamplePlaybackTime.Value = Time.Current;
                }

                rankDisplay.Rank = r.NewValue;
            }, true);
        }
    }
}
