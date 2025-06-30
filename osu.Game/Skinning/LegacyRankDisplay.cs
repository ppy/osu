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

        private Bindable<double?> lastSamplePlaybackTime = null!;

        private IBindable<ScoreRank> rank = null!;
        private ScoreRank lastRank;

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

            lastSamplePlaybackTime = statics.GetBindable<double?>(Static.LastRankChangeSamplePlaybackTime);
        }

        protected override void LoadComplete()
        {
            rank = scoreProcessor.Rank.GetBoundCopy();
            rank.BindValueChanged(r =>
            {
                var texture = source.GetTexture($"ranking-{r.NewValue}-small");

                rankDisplay.Texture = texture;

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

                bool enoughTimeElapsed = !lastSamplePlaybackTime.Value.HasValue || Time.Current - lastSamplePlaybackTime.Value >= OsuGameBase.SAMPLE_DEBOUNCE_TIME;

                // Don't play rank-down sfx on quit/retry
                if (r.NewValue != r.OldValue && r.NewValue > ScoreRank.F && PlaySamples.Value && enoughTimeElapsed)
                {
                    if (r.NewValue > lastRank)
                        rankUpSample.Play();
                    else
                        rankDownSample.Play();

                    lastSamplePlaybackTime.Value = Time.Current;
                }

                lastRank = r.NewValue;
            }, true);

            FinishTransforms(true);
        }
    }
}
