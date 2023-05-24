// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Timing.RowAttributes
{
    public partial class TimingRowAttribute : RowAttribute
    {
        private readonly BindableNumber<double> beatLength;
        private readonly Bindable<bool> omitBarLine;
        private readonly Bindable<TimeSignature> timeSignature;
        private AttributeText omitBarLineBubble = null!;
        private OsuSpriteText text = null!;

        public TimingRowAttribute(TimingControlPoint timing)
            : base(timing, "timing")
        {
            timeSignature = timing.TimeSignatureBindable.GetBoundCopy();
            omitBarLine = timing.OmitFirstBarLineBindable.GetBoundCopy();
            beatLength = timing.BeatLengthBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Content.AddRange(new[]
            {
                text = new AttributeText(Point),
                omitBarLineBubble = new AttributeText(Point) { Text = "no barline" },
            });

            Background.Colour = colourProvider.Background4;

            timeSignature.BindValueChanged(_ => updateText());
            omitBarLine.BindValueChanged(enabled => omitBarLineBubble.FadeTo(enabled.NewValue ? 1 : 0), true);
            beatLength.BindValueChanged(_ => updateText(), true);
        }

        private void updateText() =>
            text.Text = $"{60000 / beatLength.Value:n1}bpm {timeSignature.Value.GetDescription()}";
    }
}
