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
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public partial class LegacyKeyCounterDisplay : KeyCounterDisplay
    {
        private static readonly Colour4 active_colour_top = Colour4.FromHex(@"#ffde00");
        private static readonly Colour4 active_colour_bottom = Colour4.FromHex(@"#f8009e");

        protected override FillFlowContainer<KeyCounter> KeyFlow { get; }

        private readonly Sprite backgroundSprite;

        public LegacyKeyCounterDisplay()
        {
            AutoSizeAxes = Axes.Both;

            AddRange(new Drawable[]
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

            KeyTextColor = source.GetConfig<SkinCustomColourLookup, Color4>(new SkinCustomColourLookup(SkinConfiguration.LegacySetting.InputOverlayText))?.Value ?? Color4.Black;

            Texture? backgroundTexture = source.GetTexture(@"inputoverlay-background");

            if (backgroundTexture != null)
                backgroundSprite.Texture = backgroundTexture;

            for (int i = 0; i < KeyFlow.Count; ++i)
            {
                ((LegacyKeyCounter)KeyFlow[i]).ActiveColour = i < 2 ? active_colour_top : active_colour_bottom;
            }
        }

        protected override KeyCounter CreateCounter(InputTrigger trigger) => new LegacyKeyCounter(trigger)
        {
            TextColour = keyTextColor,
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
                        child.TextColour = value;
                }
            }
        }
    }
}
