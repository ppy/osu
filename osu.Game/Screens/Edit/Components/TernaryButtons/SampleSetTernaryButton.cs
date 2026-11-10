// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Components.TernaryButtons
{
    public partial class SampleSetTernaryButton : DrawableTernaryButton
    {
        public EditorBeatmapSkin.SampleSet SampleSet { get; }

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
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new HoverSounds(HoverSampleSet.Button),
            });
        }
    }
}
