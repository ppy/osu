// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Localisation.HUD;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonStrainGraph : SongProgress
    {
        [SettingSource(typeof(StrainGraphStrings), nameof(StrainGraphStrings.LineColour))]
        public BindableColour4 LineColour { get; } = new BindableColour4(Color4.White);

        [SettingSource(typeof(StrainGraphStrings), nameof(StrainGraphStrings.HorizontalSpacing))]
        public BindableInt HorizontalSpacing { get; } = new BindableInt(5)
        {
            MinValue = 3,
            MaxValue = 10,
        };

        [SettingSource(typeof(StrainGraphStrings), nameof(StrainGraphStrings.VerticalSpacing))]
        public BindableInt VerticalSpacing { get; } = new BindableInt(10)
        {
            MinValue = 5,
            MaxValue = 20,
        };

        [SettingSource(typeof(StrainGraphStrings), nameof(StrainGraphStrings.SectionGranularity), nameof(StrainGraphStrings.SectionGranularityDescription))]
        public BindableInt SectionGranularity { get; } = new BindableInt(50)
        {
            MinValue = 10,
            MaxValue = 200,
        };

        private const int highest_point = 5;

        private readonly SliderPath path = new SliderPath();
        private readonly SmoothPath drawablePath;
        private readonly Container frontContainer;
        private readonly SmoothPath frontPath;

        private IEnumerable<HitObject>? hitObjects;
        private float[] values = new float[10];

        public ArgonStrainGraph()
        {
            AutoSizeAxes = Axes.Both;

            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Masking = true;

            Child = new Container
            {
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Bottom = 3 },
                        Children = new Drawable[]
                        {
                            drawablePath = new SmoothPath
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                PathRadius = 2,
                                Alpha = 0.5f,
                            },
                            frontContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0,
                                Masking = true,
                                Child = frontPath = new SmoothPath
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    PathRadius = 2,
                                    Colour = LineColour.Value,
                                },
                            },
                        },
                    },
                },
            };
        }

        protected override void UpdateObjects(IEnumerable<HitObject> objects)
        {
            hitObjects = objects;
            refresh();
        }

        protected override void UpdateProgress(double progress, bool isIntro)
        {
            frontContainer.ResizeWidthTo(isIntro ? 0 : (float)progress, 300, Easing.OutQuint);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            HorizontalSpacing.BindValueChanged(_ => updateGraph());
            VerticalSpacing.BindValueChanged(_ => updateGraph());
            SectionGranularity.BindValueChanged(_ => refresh());

            LineColour.BindValueChanged(e => frontPath.Colour = e.NewValue);
        }

        private void refresh()
        {
            if (hitObjects == null || !hitObjects.Any())
                return;

            values = new float[SectionGranularity.Value];

            (double firstHit, double lastHit) = BeatmapExtensions.CalculatePlayableBounds(hitObjects);

            if (lastHit == 0)
                lastHit = hitObjects.Last().StartTime;

            double interval = (lastHit - firstHit + 1) / SectionGranularity.Value;

            foreach (var h in hitObjects)
            {
                double endTime = h.GetEndTime();

                Debug.Assert(endTime >= h.StartTime);

                int startRange = (int)((h.StartTime - firstHit) / interval);
                int endRange = (int)((endTime - firstHit) / interval);
                for (int i = startRange; i <= endRange; i++)
                    values[i]++;
            }

            standardizeValues();
            updateGraph();
        }

        private void standardizeValues()
        {
            // Step 1: Calculate the range of values and normalize them
            float minValue = values.Min();
            float maxValue = values.Max() - minValue;

            for (int i = 0; i < SectionGranularity.Value; i++)
            {
                values[i] = (values[i] - minValue) / maxValue * highest_point;
            }

            // Step 2: Use weighted averages to smooth each normalized value
            const int smoothing_passes = 2;

            for (int pass = 0; pass < smoothing_passes; pass++)
            {
                float[] smoothedValues = new float[SectionGranularity.Value];

                for (int i = 0; i < SectionGranularity.Value; i++)
                {
                    if (i == 0 || i == SectionGranularity.Value - 1)
                    {
                        smoothedValues[i] = values[i];
                        continue;
                    }

                    smoothedValues[i] = 0.25f * values[i - 1] + 0.5f * values[i] + 0.25f * values[i + 1];
                }

                values = smoothedValues;
            }
        }

        private void updateGraph()
        {
            path.ControlPoints.Clear();

            for (int i = 0; i < SectionGranularity.Value; i++)
            {
                path.ControlPoints.Add(new PathControlPoint
                {
                    Position = new Vector2(i * HorizontalSpacing.Value, -values[i] * VerticalSpacing.Value),
                    Type = PathType.BEZIER,
                });
            }

            List<Vector2> vertices = new List<Vector2>();
            path.GetPathToProgress(vertices, 0, 1);

            drawablePath.Vertices = vertices;
            frontPath.Vertices = vertices;
        }
    }
}
