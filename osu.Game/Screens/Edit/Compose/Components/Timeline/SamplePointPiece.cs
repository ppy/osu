// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class SamplePointPiece : HitObjectPointPiece
    {
        private readonly SampleControlPoint samplePoint;

        private readonly Bindable<string> bank;
        private readonly BindableNumber<int> volume;

        public SamplePointPiece(SampleControlPoint samplePoint)
            : base(samplePoint)
        {
            this.samplePoint = samplePoint;
            volume = samplePoint.SampleVolumeBindable.GetBoundCopy();
            bank = samplePoint.SampleBankBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            volume.BindValueChanged(volume => updateText());
            bank.BindValueChanged(bank => updateText(), true);
        }

        private void updateText()
        {
            Label.Text = $"{bank.Value} {volume.Value}";
        }
    }
}
