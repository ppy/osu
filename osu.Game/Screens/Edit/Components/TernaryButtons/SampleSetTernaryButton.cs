// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;

namespace osu.Game.Screens.Edit.Components.TernaryButtons
{
    public partial class SampleSetTernaryButton : DrawableTernaryButton
    {
        public EditorBeatmapSkin.SampleSet SampleSet { get; }

        public ISampleInfo[] DemoSamples
        {
            get => demoSample.Samples;
            set => demoSample.Samples = value;
        }

        private readonly SkinnableSound demoSample;

        public SampleSetTernaryButton(EditorBeatmapSkin.SampleSet sampleSet)
            : base(null)
        {
            SampleSet = sampleSet;
            CreateIcon = () => sampleSet.SampleSetIndex == 0
                ? new SpriteIcon { Icon = OsuIcon.SkinA }
                : new Container
                {
                    Child = new OsuSpriteText
                    {
                        Text = sampleSet.SampleSetIndex.ToString(),
                        Font = OsuFont.Style.Body.With(weight: FontWeight.Bold),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };

            switch (sampleSet.SampleSetIndex)
            {
                case 0:
                    RelativeSizeAxes = Axes.X;
                    Width = 1;
                    break;

                default:
                    RelativeSizeAxes = Axes.None;
                    Width = Height;
                    break;
            }

            demoSample = new SkinnableSound();
        }

        [BackgroundDependencyLoader]
        private void load(EditorBeatmap editorBeatmap)
        {
            AddRangeInternal(new Drawable[]
            {
                new HoverSounds(HoverSampleSet.Button),
                new EditorSkinProvidingContainer(editorBeatmap)
                {
                    Child = demoSample,
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Action = () =>
            {
                OnAction();
                demoSample.Play();
            };
        }
    }
}
