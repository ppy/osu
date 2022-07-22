// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Timing;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class SamplePointPiece : HitObjectPointPiece, IHasPopover
    {
        public readonly HitObject HitObject;

        private readonly Bindable<string> bank;
        private readonly BindableNumber<int> volume;

        public SamplePointPiece(HitObject hitObject)
            : base(hitObject.SampleControlPoint)
        {
            HitObject = hitObject;
            volume = hitObject.SampleControlPoint.SampleVolumeBindable.GetBoundCopy();
            bank = hitObject.SampleControlPoint.SampleBankBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            volume.BindValueChanged(_ => updateText());
            bank.BindValueChanged(_ => updateText(), true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            this.ShowPopover();
            return true;
        }

        private void updateText()
        {
            Label.Text = $"{bank.Value} {volume.Value}";
        }

        public Popover GetPopover() => new SampleEditPopover(HitObject);

        public class SampleEditPopover : OsuPopover
        {
            private readonly HitObject hitObject;

            private LabelledTextBox bank = null!;
            private IndeterminateSliderWithTextBoxInput<int> volume = null!;

            [Resolved(canBeNull: true)]
            private EditorBeatmap beatmap { get; set; } = null!;

            public SampleEditPopover(HitObject hitObject)
            {
                this.hitObject = hitObject;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                FillFlowContainer flow;

                Children = new Drawable[]
                {
                    flow = new FillFlowContainer
                    {
                        Width = 200,
                        Direction = FillDirection.Vertical,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            bank = new LabelledTextBox
                            {
                                Label = "Bank Name",
                            },
                            volume = new IndeterminateSliderWithTextBoxInput<int>("Volume", new SampleControlPoint().SampleVolumeBindable)
                        }
                    }
                };

                bank.TabbableContentContainer = flow;
                volume.TabbableContentContainer = flow;

                // if the piece belongs to a currently selected object, assume that the user wants to change all selected objects.
                // if the piece belongs to an unselected object, operate on that object alone, independently of the selection.
                var relevantObjects = (beatmap.SelectedHitObjects.Contains(hitObject) ? beatmap.SelectedHitObjects : hitObject.Yield()).ToArray();
                var relevantControlPoints = relevantObjects.Select(h => h.SampleControlPoint).ToArray();

                // even if there are multiple objects selected, we can still display sample volume or bank if they all have the same value.
                string? commonBank = getCommonBank(relevantControlPoints);
                if (!string.IsNullOrEmpty(commonBank))
                    bank.Current.Value = commonBank;

                int? commonVolume = getCommonVolume(relevantControlPoints);
                if (commonVolume != null)
                    volume.Current.Value = commonVolume.Value;

                updateBankPlaceholderText(relevantObjects);
                bank.Current.BindValueChanged(val =>
                {
                    updateBankFor(relevantObjects, val.NewValue);
                    updateBankPlaceholderText(relevantObjects);
                });
                // on commit, ensure that the value is correct by sourcing it from the objects' control points again.
                // this ensures that committing empty text causes a revert to the previous value.
                bank.OnCommit += (_, _) => bank.Current.Value = getCommonBank(relevantControlPoints);

                volume.Current.BindValueChanged(val => updateVolumeFor(relevantObjects, val.NewValue));
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                ScheduleAfterChildren(() => GetContainingInputManager().ChangeFocus(volume));
            }

            private static string? getCommonBank(SampleControlPoint[] relevantControlPoints) => relevantControlPoints.Select(point => point.SampleBank).Distinct().Count() == 1 ? relevantControlPoints.First().SampleBank : null;
            private static int? getCommonVolume(SampleControlPoint[] relevantControlPoints) => relevantControlPoints.Select(point => point.SampleVolume).Distinct().Count() == 1 ? relevantControlPoints.First().SampleVolume : null;

            private void updateBankFor(IEnumerable<HitObject> objects, string? newBank)
            {
                if (string.IsNullOrEmpty(newBank))
                    return;

                beatmap.BeginChange();

                foreach (var h in objects)
                {
                    h.SampleControlPoint.SampleBank = newBank;
                    beatmap.Update(h);
                }

                beatmap.EndChange();
            }

            private void updateBankPlaceholderText(IEnumerable<HitObject> objects)
            {
                string? commonBank = getCommonBank(objects.Select(h => h.SampleControlPoint).ToArray());
                bank.PlaceholderText = string.IsNullOrEmpty(commonBank) ? "(multiple)" : string.Empty;
            }

            private void updateVolumeFor(IEnumerable<HitObject> objects, int? newVolume)
            {
                if (newVolume == null)
                    return;

                beatmap.BeginChange();

                foreach (var h in objects)
                {
                    h.SampleControlPoint.SampleVolume = newVolume.Value;
                    beatmap.Update(h);
                }

                beatmap.EndChange();
            }
        }
    }
}
