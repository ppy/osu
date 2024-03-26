// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Screens.Edit.Timing
{
    internal partial class EffectSection : Section<EffectControlPoint>
    {
        private LabelledSwitchButton kiai = null!;

        private SliderWithTextBoxInput<double> scrollSpeedSlider = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new Drawable[]
            {
                kiai = new LabelledSwitchButton { Label = "Kiai Time" },
                scrollSpeedSlider = new SliderWithTextBoxInput<double>("Scroll Speed")
                {
                    Current = new EffectControlPoint().ScrollSpeedBindable,
                    KeyboardStep = 0.1f
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            kiai.Current.BindValueChanged(_ => saveChanges());
            scrollSpeedSlider.Current.BindValueChanged(_ => saveChanges());

            var drawableRuleset = Beatmap.BeatmapInfo.Ruleset.CreateInstance().CreateDrawableRulesetWith(Beatmap.PlayableBeatmap);
            if (drawableRuleset is not IDrawableScrollingRuleset scrollingRuleset || scrollingRuleset.VisualisationMethod == ScrollVisualisationMethod.Constant)
                scrollSpeedSlider.Hide();

            void saveChanges()
            {
                if (!isRebinding) ChangeHandler?.SaveState();
            }
        }

        private bool isRebinding;

        protected override void OnControlPointChanged(ValueChangedEvent<EffectControlPoint?> point)
        {
            scrollSpeedSlider.Current.ValueChanged -= updateControlPointFromSlider;

            if (point.NewValue is EffectControlPoint newEffectPoint)
            {
                isRebinding = true;

                kiai.Current = newEffectPoint.KiaiModeBindable;
                scrollSpeedSlider.Current = new BindableDouble
                {
                    MinValue = 0.01,
                    MaxValue = 10,
                    Precision = 0.01,
                    Value = newEffectPoint.ScrollSpeedBindable.Value
                };
                scrollSpeedSlider.Current.ValueChanged += updateControlPointFromSlider;
                // at this point in time the above is enough to keep the slider control in sync with reality,
                // since undo/redo causes `OnControlPointChanged()` to fire.
                // whenever that stops being the case, or there is a possibility that the scroll speed could be changed
                // by something else other than this control, this code should probably be revisited to have a binding in the other direction, too.

                isRebinding = false;
            }
        }

        private void updateControlPointFromSlider(ValueChangedEvent<double> scrollSpeed)
        {
            if (ControlPoint.Value is not EffectControlPoint effectPoint || isRebinding)
                return;

            effectPoint.ScrollSpeedBindable.Value = scrollSpeed.NewValue;
        }

        protected override EffectControlPoint CreatePoint()
        {
            var reference = Beatmap.ControlPointInfo.EffectPointAt(SelectedGroup.Value.Time);

            return new EffectControlPoint
            {
                KiaiMode = reference.KiaiMode,
                ScrollSpeed = reference.ScrollSpeed,
            };
        }
    }
}
