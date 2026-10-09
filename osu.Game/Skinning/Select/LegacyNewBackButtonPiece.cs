// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning.Select
{
    /// <summary>
    /// Represents a port of the default osu!stable back button.
    /// </summary>
    /// <seealso href="https://github.com/peppy/osu-stable-reference/blob/c34a74fb61c17c5667486a12548485d1f03baa2e/osu!/Graphics/UserInterface/BackButton.cs">osu!stable reference source</seealso>
    public partial class LegacyNewBackButtonPiece : BeatSyncedContainer
    {
        private static readonly Color4 idle_colour = new Color4(238, 51, 153, 255);
        private static readonly Color4 active_colour = new Color4(187, 17, 119, 255);

        private SkinnableSound hoverSound = null!;
        private SkinnableSound clickSound = null!;

        private Sprite leftLayer = null!;
        private SpriteIcon icon = null!;
        private OsuSpriteText text = null!;
        private Sprite rightLayer = null!;

        private const float back_text_size = 15 * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR;
        private const float vertical_offset = 22 * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR;

        // 0.775x is an arbitrary adjustment to better match visually.
        // on testing with stable this factor is only relevant to fontawesome icon sizes.
        private const float back_icon_size = 18 * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR * 0.775f;

        [BackgroundDependencyLoader]
        private void load(SkinManager skins)
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                rightLayer = new Sprite
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomRight,
                    Texture = skins.DefaultClassicSkin.GetTexture("back-button-layer"),
                    Colour = idle_colour,
                },
                text = new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.Centre,
                    Text = @"back",
                    Font = OsuFont.Default.With(size: back_text_size),
                    Y = -vertical_offset,
                    UseFullGlyphHeight = false,
                },
                leftLayer = new Sprite
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomRight,
                    Texture = skins.DefaultClassicSkin.GetTexture("back-button-layer"),
                    Colour = idle_colour,
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Solid.ChevronCircleLeft,
                    Size = new Vector2(back_icon_size),
                    Y = -vertical_offset,
                    Shadow = true,
                },
            };

            AddInternal(hoverSound = new SkinnableSound(new SampleInfo(@"back-button-hover", @"menuclick")));
            AddInternal(clickSound = new SkinnableSound(new SampleInfo(@"back-button-click", @"menuback")));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateDisplay();
            FinishTransforms(true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverSound.Play();
            updateDisplay();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateDisplay();
        }

        protected override bool OnClick(ClickEvent e)
        {
            clickSound.Play();
            contract(400, Easing.InOutBack);
            return base.OnClick(e);
        }

        private void updateDisplay()
        {
            if (IsHovered)
                expand(600, Easing.OutElastic);
            else
                contract(600, Easing.OutElastic);
        }

        private const float centre_offset = 2.4f;

        private const float left_layer_idle = 22 * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR + 2; // +2 to better match stable
        private const float left_layer_active = 40 * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR + 3; // +3 to better match stable

        private const float right_layer_idle = 60 * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR + 5; // +5 to better match stable
        private const float right_layer_active = 86 * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR + 7; // +7 to better match stable

        private const float back_text_width = 45; // determined via draw visualizer

        private static readonly float back_text_allowance = Math.Max(0, back_text_width - (right_layer_idle - left_layer_idle));

        private void expand(double duration, Easing easing)
        {
            leftLayer.MoveToX(left_layer_active, duration, easing);
            leftLayer.FadeColour(active_colour, duration, easing);
            icon.MoveToX(left_layer_active / centre_offset, duration, easing);
            rightLayer.MoveToX(right_layer_active + back_text_allowance, duration, easing);
            text.MoveToX((right_layer_active - left_layer_active + back_text_allowance) / centre_offset + left_layer_active, duration, easing);
        }

        private void contract(double duration, Easing easing)
        {
            leftLayer.MoveToX(left_layer_idle, duration, easing);
            leftLayer.FadeColour(idle_colour, duration, easing);
            icon.MoveToX(left_layer_idle / centre_offset, duration, easing);
            rightLayer.MoveToX(right_layer_idle + back_text_allowance, duration, easing);
            text.MoveToX((right_layer_idle - left_layer_idle + back_text_allowance) / centre_offset + left_layer_idle, duration, easing);
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            icon.ScaleTo(IsHovered ? 1.5f : 0.9f);

            using (BeginDelayedSequence(-timingPoint.BeatLength / 4))
                icon.ScaleTo(1, timingPoint.BeatLength * 3, Easing.OutElastic);
        }
    }
}
