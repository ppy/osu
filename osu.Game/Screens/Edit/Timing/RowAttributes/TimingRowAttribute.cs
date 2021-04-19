// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Timing.RowAttributes
{
    internal class TimingRowAttribute : RowAttribute
    {
        private readonly BindableNumber<double> beatLength;
        private readonly Bindable<TimeSignatures> timeSignature;
        private OsuSpriteText text;

        public TimingRowAttribute(TimingControlPoint timing)
            : base(timing)
        {
            timeSignature = timing.TimeSignatureBindable.GetBoundCopy();
            beatLength = timing.BeatLengthBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.Add(text = new OsuSpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Font = OsuFont.GetFont(size: EditorTable.TEXT_SIZE, weight: FontWeight.Regular),
            });

            timeSignature.BindValueChanged(_ => updateText());
            beatLength.BindValueChanged(_ => updateText(), true);
        }

        private void updateText() =>
            text.Text = $"{60000 / beatLength.Value:n1}bpm {timeSignature.Value.GetDescription()}";
    }
}
