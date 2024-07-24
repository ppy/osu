// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.HUD;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Skinning
{
    public partial class LegacyKeyCounterDisplay : KeyCounterDisplay
    {
        private const float key_transition_time = 100;

        protected override FillFlowContainer<KeyCounter> KeyFlow { get; }

        private readonly Sprite backgroundSprite;

        public LegacyKeyCounterDisplay()
        {
            AutoSizeAxes = Axes.Both;

            AddRangeInternal(new Drawable[]
            {
                backgroundSprite = new Sprite
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                KeyFlow = new FillFlowContainer<KeyCounter>
                {
                    // https://osu.ppy.sh/wiki/en/Skinning/Interface#input-overlay
                    // 24px away from the container, there're 4 counter in legacy, so divide by 4
                    // "inputoverlay-background.png" are 1.05x in-game. so *1.05f to the X coordinate
                    X = 24f / 4f,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Direction = FillDirection.Horizontal,
                    AutoSizeAxes = Axes.Both,
                },
            });
        }

        [Resolved]
        private ISkinSource source { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            source.GetConfig<SkinConfiguration.LegacySetting, Colour4>(SkinConfiguration.LegacySetting.InputOverlayText)?.BindValueChanged(v =>
            {
                KeyTextColor = v.NewValue;
            }, true);

            Texture? backgroundTexture = source.GetTexture(@"inputoverlay-background");

            if (backgroundTexture != null)
                backgroundSprite.Texture = backgroundTexture;
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

        private Colour4 keyTextColor = Colour4.White;

        public Colour4 KeyTextColor
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
