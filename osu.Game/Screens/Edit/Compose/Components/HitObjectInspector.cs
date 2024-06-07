// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Threading;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Compose.Components
{
    internal partial class HitObjectInspector : EditorInspector
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            EditorBeatmap.SelectedHitObjects.CollectionChanged += (_, _) => updateInspectorText();
            EditorBeatmap.TransactionBegan += updateInspectorText;
            EditorBeatmap.TransactionEnded += updateInspectorText;
            updateInspectorText();
        }

        private ScheduledDelegate? rollingTextUpdate;

        private void updateInspectorText()
        {
            InspectorText.Clear();
            rollingTextUpdate?.Cancel();
            rollingTextUpdate = null;

            switch (EditorBeatmap.SelectedHitObjects.Count)
            {
                case 0:
                    AddValue("No selection");
                    break;

                case 1:
                    var selected = EditorBeatmap.SelectedHitObjects.Single();

                    AddHeader("Type");
                    AddValue($"{selected.GetType().ReadableName()}");

                    AddHeader("Time");
                    AddValue($"{selected.StartTime:#,0.##}ms");

                    switch (selected)
                    {
                        case IHasPosition pos:
                            AddHeader("Position");
                            AddValue($"x:{pos.X:#,0.##} y:{pos.Y:#,0.##}");
                            break;

                        case IHasXPosition x:
                            AddHeader("Position");

                            AddValue($"x:{x.X:#,0.##} ");
                            break;

                        case IHasYPosition y:
                            AddHeader("Position");

                            AddValue($"y:{y.Y:#,0.##}");
                            break;
                    }

                    if (selected is IHasDistance distance)
                    {
                        AddHeader("Distance");
                        AddValue($"{distance.Distance:#,0.##}px");
                    }

                    if (selected is IHasSliderVelocity sliderVelocity)
                    {
                        AddHeader("Slider Velocity");
                        AddValue($"{sliderVelocity.SliderVelocityMultiplier:#,0.00}x ({sliderVelocity.SliderVelocityMultiplier * EditorBeatmap.Difficulty.SliderMultiplier:#,0.00}x)");
                    }

                    if (selected is IHasRepeats repeats)
                    {
                        AddHeader("Repeats");
                        AddValue($"{repeats.RepeatCount:#,0.##}");
                    }

                    if (selected is IHasDuration duration)
                    {
                        AddHeader("End Time");
                        AddValue($"{duration.EndTime:#,0.##}ms");
                        AddHeader("Duration");
                        AddValue($"{duration.Duration:#,0.##}ms");
                    }

                    // I'd hope there's a better way to do this, but I don't want to bind to each and every property above to watch for changes.
                    // This is a good middle-ground for the time being.
                    rollingTextUpdate ??= Scheduler.AddDelayed(updateInspectorText, 250);
                    break;

                default:
                    AddHeader("Selected Objects");
                    AddValue($"{EditorBeatmap.SelectedHitObjects.Count:#,0.##}");

                    AddHeader("Start Time");
                    AddValue($"{EditorBeatmap.SelectedHitObjects.Min(o => o.StartTime):#,0.##}ms");

                    AddHeader("End Time");
                    AddValue($"{EditorBeatmap.SelectedHitObjects.Max(o => o.GetEndTime()):#,0.##}ms");
                    break;
            }
        }
    }
}
