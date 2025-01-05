// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    public partial class ControlPointVisualisation : PointVisualisation, IControlPointVisualisation, IHasTooltip
    {
        protected readonly ControlPoint Point;

        public ControlPointVisualisation(ControlPoint point)
            : base(point.Time)
        {
            Point = point;
            Alpha = 0.5f;
            Blending = BlendingParameters.Additive;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = Point.GetRepresentingColour(colours);
        }

        public bool IsVisuallyRedundant(ControlPoint other) => other.GetType() == Point.GetType();

        public LocalisableString TooltipText
        {
            get
            {
                switch (Point)
                {
                    case EffectControlPoint effect:
                        return $"{StartTime.ToEditorFormattedString()} effect [{effect.ScrollSpeed:N2}x scroll{(effect.KiaiMode ? " kiai" : "")}]";

                    case TimingControlPoint timing:
                        return $"{StartTime.ToEditorFormattedString()} timing [{timing.BPM:N2} bpm {timing.TimeSignature.GetDescription()}]";
                }

                return string.Empty;
            }
        }
    }
}
