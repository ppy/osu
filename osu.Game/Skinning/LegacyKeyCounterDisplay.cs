// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.HUD;
using osuTK.Graphics;
using osu.Framework.Allocation;

namespace osu.Game.Skinning
{
    public partial class LegacyKeyCounterDisplay : KeyCounterDisplay
    {
        private const float key_transition_time = 50;

        protected override FillFlowContainer<KeyCounter> KeyFlow { get; } = null!;

        private SkinnableSprite overlayBackground = null!;

        public LegacyKeyCounterDisplay()
        {
            AutoSizeAxes = Axes.Both;

            AddRangeInternal(new Drawable[]
            {
                 overlayBackground = new SkinnableSprite
                 {
                     Anchor = Anchor.TopLeft,
                     Origin = Anchor.TopLeft,
                     //BypassAutoSizeAxes = Axes.Both,
                     SpriteName = { Value= "inputoverlay-background" },
                 },
                 KeyFlow = new FillFlowContainer<KeyCounter>
                 {
                     Padding = new MarginPadding
                     {
                         Horizontal = 7f * 1.05f,
                     },
                     Anchor = Anchor.TopLeft,
                     Origin = Anchor.TopLeft,
                     Direction = FillDirection.Horizontal,
                     AutoSizeAxes = Axes.Both,
                 },
            });
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource source)
        {
            source.GetConfig<string, Colour4>("InputOverlayText")?.BindValueChanged(v =>
            {
                KeyTextColor = v.NewValue;
            }, true);
        }

        protected override KeyCounter CreateCounter(InputTrigger trigger) => new LegacyKeyCounter(trigger)
        {
            TransitionDuration = key_transition_time,
            KeyTextColour = keyTextColor,
        };

        private Color4 keyTextColor = Color4.White;

        public Color4 KeyTextColor
        {
            get => keyTextColor;
            set
            {
                if (value != keyTextColor)
                {
                    keyTextColor = value;
                    foreach (var child in KeyFlow.Cast<LegacyKeyCounter>())
                        child.KeyTextColour = value;
                }
            }
        }
    }
}
