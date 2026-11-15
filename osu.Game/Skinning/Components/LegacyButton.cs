// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Skinning.Components
{
    /// <seealso href="https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/Graphics/UserInterface/pButton.cs"/>
    public partial class LegacyButton : CompositeDrawable
    {
        private NineSliceSprite backgroundSprite = null!;
        private PoolableSkinnableSample hoverSound = null!;
        private PoolableSkinnableSample clickSound = null!;

        public required Colour4 AccentColour { get; init; }

        public required LocalisableString Text { get; init; }

        public new required Vector2 Size
        {
            get => base.Size;
            init => base.Size = value;
        }

        public Action? Action { get; set; }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            var leftSprite = skin.GetTexture(@"button-left");
            float leftInset = leftSprite != null ? leftSprite.Width / leftSprite.ScaleAdjust : 0;

            var rightSprite = skin.GetTexture(@"button-right");
            float rightInset = rightSprite != null ? rightSprite.Width / rightSprite.ScaleAdjust : 0;

            InternalChildren =
            [
                backgroundSprite = new NineSliceSprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = skin.GetTexture(@"button"),
                    TextureInset = new MarginPadding
                    {
                        Left = leftInset,
                        Right = rightInset,
                    },
                },
                // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/Graphics/UserInterface/pButton.cs#L128
                hoverSound = new PoolableSkinnableSample(new SampleInfo(@"click-short")),
                // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/Graphics/UserInterface/pButton.cs#L113
                clickSound = new PoolableSkinnableSample(new SampleInfo(@"click-short-confirm")),
                new OsuSpriteText
                {
                    Text = Text,
                    // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/Graphics/UserInterface/pButton.cs#L53
                    Font = OsuFont.Inter.With(size: 14 * Height / 18),
                    UseFullGlyphHeight = false,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateState();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            hoverSound.Play();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            clickSound.Play();
            // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/Graphics/UserInterface/pButton.cs#L114
            backgroundSprite.FlashColour(Colour4.White, 400);
            return true;
        }

        private void updateState()
        {
            // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/Graphics/UserInterface/pButton.cs#L170-L182
            var targetColour = AccentColour;
            const float unhovered_reduction = 20 / 255f;

            if (!IsHovered)
            {
                targetColour = new Colour4(
                    MathF.Max(0, targetColour.R - unhovered_reduction),
                    MathF.Max(0, targetColour.G - unhovered_reduction),
                    MathF.Max(0, targetColour.B - unhovered_reduction),
                    targetColour.A);
            }

            backgroundSprite.Colour = targetColour;
        }
    }
}
