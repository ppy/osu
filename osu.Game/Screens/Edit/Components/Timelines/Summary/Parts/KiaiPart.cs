// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Extensions;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// The part of the timeline that displays kiai sections in the song.
    /// </summary>
    public partial class KiaiPart : TimelinePart
    {
        private DrawablePool<KiaiVisualisation> pool = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(pool = new DrawablePool<KiaiVisualisation>(10));
        }

        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);
            EditorBeatmap.ControlPointInfo.ControlPointsChanged += updateParts;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateParts();
        }

        private void updateParts() => Scheduler.AddOnce(() =>
        {
            Clear(disposeChildren: false);

            double? startTime = null;

            foreach (var effectPoint in EditorBeatmap.ControlPointInfo.EffectPoints)
            {
                if (startTime.HasValue)
                {
                    if (effectPoint.KiaiMode)
                        continue;

                    var section = new KiaiSection
                    {
                        StartTime = startTime.Value,
                        EndTime = effectPoint.Time
                    };

                    Add(pool.Get(v => v.Section = section));

                    startTime = null;
                }
                else
                {
                    if (!effectPoint.KiaiMode)
                        continue;

                    startTime = effectPoint.Time;
                }
            }

            // last effect point has kiai enabled, kiai should last until the end of the map
            if (startTime.HasValue)
            {
                Add(pool.Get(v => v.Section = new KiaiSection
                {
                    StartTime = startTime.Value,
                    EndTime = Content.RelativeChildSize.X
                }));
            }
        });

        private partial class KiaiVisualisation : PoolableDrawable, IHasTooltip
        {
            private KiaiSection section;

            public KiaiSection Section
            {
                set
                {
                    section = value;

                    X = (float)value.StartTime;
                    Width = (float)value.Duration;
                }
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativePositionAxes = Axes.X;
                RelativeSizeAxes = Axes.Both;
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                Height = 0.2f;
                AddInternal(new FastCircle
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Purple1
                });
            }

            public LocalisableString TooltipText => $"{section.StartTime.ToEditorFormattedString()} - {section.EndTime.ToEditorFormattedString()} kiai time";
        }

        private readonly struct KiaiSection
        {
            public double StartTime { get; init; }
            public double EndTime { get; init; }
            public double Duration => EndTime - StartTime;
        }
    }
}
