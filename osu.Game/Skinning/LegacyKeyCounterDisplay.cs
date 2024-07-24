// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.HUD;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

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
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopLeft,
                    Scale = new Vector2(1.05f, 1),
                    Rotation = 90,
                },
                KeyFlow = new FillFlowContainer<KeyCounter>
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    X = -1.5f,
                    Y = 7,
                    Spacing = new Vector2(1.8f),
                    Direction = FillDirection.Vertical,
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
        };

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
