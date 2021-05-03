// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Timing.RowAttributes
{
    public class EffectRowAttribute : RowAttribute
    {
        private readonly Bindable<bool> kiaiMode;
        private readonly Bindable<bool> omitBarLine;
        private AttributeText kiaiModeBubble;
        private AttributeText omitBarLineBubble;

        public EffectRowAttribute(EffectControlPoint effect)
            : base(effect, "effect")
        {
            kiaiMode = effect.KiaiModeBindable.GetBoundCopy();
            omitBarLine = effect.OmitFirstBarLineBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.AddRange(new Drawable[]
            {
                kiaiModeBubble = new AttributeText(Point) { Text = "kiai" },
                omitBarLineBubble = new AttributeText(Point) { Text = "no barline" },
            });

            kiaiMode.BindValueChanged(enabled => kiaiModeBubble.FadeTo(enabled.NewValue ? 1 : 0), true);
            omitBarLine.BindValueChanged(enabled => omitBarLineBubble.FadeTo(enabled.NewValue ? 1 : 0), true);
        }
    }
}
