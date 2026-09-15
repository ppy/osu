// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class FlowKeyCounterDisplay : KeyCounterDisplay
    {
        [SettingSource(typeof(FlowKeyCounterDisplayStrings), nameof(FlowKeyCounterDisplayStrings.ShowKeyName))]
        public BindableBool ShowTriggerName { get; } = new BindableBool(true);

        [SettingSource(typeof(FlowKeyCounterDisplayStrings), nameof(FlowKeyCounterDisplayStrings.FlowDuration))]
        public BindableDouble FlowDuration { get; } = new BindableDouble(500)
        {
            MinValue = 250,
            MaxValue = 2000,
            Precision = 1,
        };

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.Colour))]
        public BindableColour4 AccentColour { get; } = new BindableColour4(Colour4.White);

        protected override FillFlowContainer<KeyCounter> KeyFlow { get; }

        public FlowKeyCounterDisplay()
        {
            Child = KeyFlow = new FillFlowContainer<KeyCounter>
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(2),
            };
        }

        protected override KeyCounter CreateCounter(InputTrigger trigger)
        {
            var counter = new FlowKeyCounter(trigger);
            counter.ShowTriggerName.BindTo(ShowTriggerName);
            counter.FlowDuration.BindTo(FlowDuration);
            counter.AccentColour.BindTo(AccentColour);

            return counter;
        }
    }
}
