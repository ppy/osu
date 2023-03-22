// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Timing.RowAttributes
{
    public partial class SampleRowAttribute : RowAttribute
    {
        private AttributeText sampleText = null!;
        private OsuSpriteText volumeText = null!;

        private readonly Bindable<string> sampleBank;
        private readonly BindableNumber<int> volume;

        public SampleRowAttribute(SampleControlPoint sample)
            : base(sample, "sample")
        {
            sampleBank = sample.SampleBankBindable.GetBoundCopy();
            volume = sample.SampleVolumeBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AttributeProgressBar progress;

            Content.AddRange(new Drawable[]
            {
                sampleText = new AttributeText(Point),
                progress = new AttributeProgressBar(Point),
                volumeText = new AttributeText(Point)
                {
                    Width = 40,
                },
            });

            volume.BindValueChanged(vol =>
            {
                progress.Current.Value = vol.NewValue / 100f;
                updateText();
            }, true);

            sampleBank.BindValueChanged(_ => updateText(), true);
        }

        private void updateText()
        {
            volumeText.Text = $"{volume.Value}%";
            sampleText.Text = $"{sampleBank.Value}";
        }
    }
}
