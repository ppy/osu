// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Timing.RowAttributes
{
    public partial class EffectRowAttribute : RowAttribute
    {
        private readonly Bindable<bool> kiaiMode;
        private readonly BindableNumber<double> scrollSpeed;

        private AttributeText kiaiModeBubble = null!;
        private AttributeText text = null!;

        public EffectRowAttribute(EffectControlPoint effect)
            : base(effect, "effect")
        {
            kiaiMode = effect.KiaiModeBindable.GetBoundCopy();
            scrollSpeed = effect.ScrollSpeedBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.AddRange(new Drawable[]
            {
                new AttributeProgressBar(Point)
                {
                    Current = scrollSpeed,
                },
                text = new AttributeText(Point) { Width = 45 },
                kiaiModeBubble = new AttributeText(Point) { Text = "kiai" },
            });

            kiaiMode.BindValueChanged(enabled => kiaiModeBubble.FadeTo(enabled.NewValue ? 1 : 0), true);
            scrollSpeed.BindValueChanged(_ => updateText(), true);
        }

        private void updateText() => text.Text = $"{scrollSpeed.Value:n2}x";
    }
}
