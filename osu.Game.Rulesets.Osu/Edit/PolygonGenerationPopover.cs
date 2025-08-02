// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class PolygonGenerationPopover : OsuPopover
    {
        private SliderWithTextBoxInput<double> distanceSnapInput = null!;
        private SliderWithTextBoxInput<int> offsetAngleInput = null!;
        private SliderWithTextBoxInput<int> repeatCountInput = null!;
        private SliderWithTextBoxInput<int> pointInput = null!;
        private RoundedButton commitButton = null!;

        private readonly List<HitCircle> insertedCircles = new List<HitCircle>();
        private bool began;
        private bool committed;

        [Resolved]
        private IBeatSnapProvider beatSnapProvider { get; set; } = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

        [Resolved]
        private HitObjectComposer composer { get; set; } = null!;

        private Bindable<TernaryState> newComboState = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            var selectionHandler = (EditorSelectionHandler)composer.BlueprintContainer.SelectionHandler;
            newComboState = selectionHandler.SelectionNewComboState.GetBoundCopy();

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };

            Child = new FillFlowContainer
            {
                Width = 220,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(20),
                Children = new Drawable[]
                {
                    distanceSnapInput = new SliderWithTextBoxInput<double>("Distance snap:")
                    {
                        Current = new BindableNumber<double>(1)
                        {
                            MinValue = 0.1,
                            MaxValue = 6,
                            Precision = 0.1,
                            Value = ((OsuHitObjectComposer)composer).DistanceSnapProvider.DistanceSpacingMultiplier.Value,
                        },
                        Instantaneous = true
                    },
                    offsetAngleInput = new SliderWithTextBoxInput<int>("Offset angle:")
                    {
                        Current = new BindableNumber<int>
                        {
                            MinValue = 0,
                            MaxValue = 180,
                            Precision = 1
                        },
                        Instantaneous = true
                    },
                    repeatCountInput = new SliderWithTextBoxInput<int>("Repeats:")
                    {
                        Current = new BindableNumber<int>(1)
                        {
                            MinValue = 1,
                            MaxValue = 10,
                            Precision = 1
                        },
                        Instantaneous = true
                    },
                    pointInput = new SliderWithTextBoxInput<int>("Vertices:")
                    {
                        Current = new BindableNumber<int>(3)
                        {
                            MinValue = 3,
                            MaxValue = 32,
                            Precision = 1,
                        },
                        Instantaneous = true
                    },
                    commitButton = new RoundedButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Create",
                        Action = commit
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            changeHandler?.BeginChange();
            began = true;

            distanceSnapInput.Current.BindValueChanged(_ => Scheduler.AddOnce(tryCreatePolygon));
            offsetAngleInput.Current.BindValueChanged(_ => Scheduler.AddOnce(tryCreatePolygon));
            repeatCountInput.Current.BindValueChanged(_ => Scheduler.AddOnce(tryCreatePolygon));
            pointInput.Current.BindValueChanged(_ => Scheduler.AddOnce(tryCreatePolygon));
            newComboState.BindValueChanged(_ => Scheduler.AddOnce(tryCreatePolygon));
            tryCreatePolygon();
        }

        private void tryCreatePolygon()
        {
            double startTime = beatSnapProvider.SnapTime(editorClock.CurrentTime);
            TimingControlPoint timingPoint = editorBeatmap.ControlPointInfo.TimingPointAt(startTime);
            double timeSpacing = timingPoint.BeatLength / editorBeatmap.BeatDivisor;
            IHasSliderVelocity lastWithSliderVelocity = editorBeatmap.HitObjects.Where(ho => ho.GetEndTime() <= startTime).OfType<IHasSliderVelocity>().LastOrDefault() ?? new Slider();
            double velocity = OsuHitObject.BASE_SCORING_DISTANCE * editorBeatmap.Difficulty.SliderMultiplier
                              / LegacyRulesetExtensions.GetPrecisionAdjustedBeatLength(lastWithSliderVelocity, timingPoint, OsuRuleset.SHORT_NAME);
            double length = distanceSnapInput.Current.Value * velocity * timeSpacing;
            float polygonRadius = (float)(length / (2 * Math.Sin(double.Pi / pointInput.Current.Value)));

            int totalPoints = pointInput.Current.Value * repeatCountInput.Current.Value;

            if (insertedCircles.Count > totalPoints)
            {
                editorBeatmap.RemoveRange(insertedCircles.GetRange(totalPoints, insertedCircles.Count - totalPoints));
                insertedCircles.RemoveRange(totalPoints, insertedCircles.Count - totalPoints);
            }

            var newlyAdded = new List<HitCircle>();

            for (int i = 0; i < totalPoints; ++i)
            {
                float angle = float.DegreesToRadians(offsetAngleInput.Current.Value) + (i + 1) * (2 * float.Pi / pointInput.Current.Value);
                var position = OsuPlayfield.BASE_SIZE / 2 + new Vector2(polygonRadius * float.Cos(angle), polygonRadius * float.Sin(angle));
                bool newCombo = i == 0 && newComboState.Value == TernaryState.True;

                HitCircle circle;

                if (i < insertedCircles.Count)
                {
                    circle = insertedCircles[i];

                    circle.Position = position;
                    circle.StartTime = startTime;
                    circle.NewCombo = newCombo;

                    editorBeatmap.Update(circle);
                }
                else
                {
                    circle = new HitCircle
                    {
                        Position = position,
                        StartTime = startTime,
                        NewCombo = newCombo,
                    };

                    newlyAdded.Add(circle);

                    // TODO: probably ensure samples also follow current ternary status (not trivial)
                    circle.Samples.Add(circle.CreateHitSampleInfo());
                }

                if (position.X < 0 || position.Y < 0 || position.X > OsuPlayfield.BASE_SIZE.X || position.Y > OsuPlayfield.BASE_SIZE.Y)
                {
                    commitButton.Enabled.Value = false;
                    editorBeatmap.RemoveRange(insertedCircles);
                    insertedCircles.Clear();
                    return;
                }

                startTime = beatSnapProvider.SnapTime(startTime + timeSpacing);
            }

            var previousNewComboState = newComboState.Value;

            insertedCircles.AddRange(newlyAdded);
            editorBeatmap.AddRange(newlyAdded);

            // When adding new hitObjects, newCombo state will get reset to false when no objects are selected.
            // Since this is the case when this popover is showing, we need to restore the previous newCombo state
            newComboState.Value = previousNewComboState;

            commitButton.Enabled.Value = true;
        }

        private void commit()
        {
            changeHandler?.EndChange();
            committed = true;
            Hide();
        }

        protected override void PopOut()
        {
            base.PopOut();

            if (began && !committed)
            {
                editorBeatmap.RemoveRange(insertedCircles);
                changeHandler?.EndChange();
            }
        }
    }
}
