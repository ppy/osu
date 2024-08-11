// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Extensions;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    public partial class EffectPointVisualisation : CompositeDrawable, IControlPointVisualisation
    {
        private readonly EffectControlPoint effect;
        private Bindable<bool> kiai = null!;

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public EffectPointVisualisation(EffectControlPoint point)
        {
            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Y;

            effect = point;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            kiai = effect.KiaiModeBindable.GetBoundCopy();
            kiai.BindValueChanged(_ => refreshDisplay(), true);
        }

        private EffectControlPoint? nextControlPoint;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Due to the limitations of ControlPointInfo, it's impossible to know via event flow when the next kiai point has changed.
            // This is due to the fact that an EffectPoint can be added to an existing group. We would need to bind to ItemAdded on *every*
            // future group to track this.
            //
            // I foresee this being a potential performance issue on beatmaps with many control points, so let's limit how often we check
            // for changes. ControlPointInfo needs a refactor to make this flow better, but it should do for now.
            Scheduler.AddDelayed(() =>
            {
                EffectControlPoint? next = null;

                int activePointIndex = ControlPointInfo.BinarySearch(beatmap.ControlPointInfo.EffectPoints, effect.Time).Index;
                if (activePointIndex + 1 <= beatmap.ControlPointInfo.EffectPoints.Count - 1)
                    next = beatmap.ControlPointInfo.EffectPoints[activePointIndex + 1];

                if (!ReferenceEquals(nextControlPoint, next))
                {
                    nextControlPoint = next;
                    refreshDisplay();
                }
            }, 100, true);
        }

        private void refreshDisplay()
        {
            ClearInternal();

            AddInternal(new ControlPointVisualisation(effect));

            if (!kiai.Value)
                return;

            // handle kiai duration
            // eventually this will be simpler when we have control points with durations.
            if (nextControlPoint != null)
            {
                RelativeSizeAxes = Axes.Both;
                Origin = Anchor.TopLeft;

                Width = (float)(nextControlPoint.Time - effect.Time);

                AddInternal(new KiaiVisualisation(effect.Time, nextControlPoint.Time)
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.CentreLeft,
                    Height = 0.4f,
                    Depth = float.MaxValue,
                    Colour = colours.Purple1,
                });
            }
        }

        private partial class KiaiVisualisation : Circle, IHasTooltip
        {
            private readonly double startTime;
            private readonly double endTime;

            public KiaiVisualisation(double startTime, double endTime)
            {
                this.startTime = startTime;
                this.endTime = endTime;
            }

            public LocalisableString TooltipText => $"{startTime.ToEditorFormattedString()} - {endTime.ToEditorFormattedString()} kiai time";
        }

        // kiai sections display duration, so are required to be visualised.
        public bool IsVisuallyRedundant(ControlPoint other) => other is EffectControlPoint otherEffect && effect.KiaiMode == otherEffect.KiaiMode;
    }
}
