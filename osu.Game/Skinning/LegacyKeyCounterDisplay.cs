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

        public LegacyKeyCounterDisplay()
        {
            AutoSizeAxes = Axes.Both;

            AddRangeInternal(new Drawable[]
            {
                 new SkinnableSprite
                 {
                     Anchor = Anchor.TopLeft,
                     Origin = Anchor.TopLeft,
                     SpriteName = { Value= "inputoverlay-background" },
                 },
                 KeyFlow = new FillFlowContainer<KeyCounter>
                 {
                     // https://osu.ppy.sh/wiki/en/Skinning/Interface#input-overlay
                     // 24px away from the container, there're 4 counter in legacy, so divide by 4
                     // "inputoverlay-background.png" are 1.05x in-game. so *1.05f to the X coordinate
                     X = (24 / 4) * 1.05f,
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
            KeyTextRotation = -Rotation,
        };

        protected override void Update()
        {
            base.Update();
            // keep the text are always horizontal
            foreach (var child in KeyFlow.Cast<LegacyKeyCounter>())
                child.KeyTextRotation = -Rotation;
        }

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
