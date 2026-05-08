// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
                    Font = OsuFont.Default.With(size: 26),
                    Y = -35,
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
                    Size = new Vector2(24),
                    Y = -35,
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

        private const float right_layer_idle = 95;
        private const float right_layer_active = 140;

        private const float centre_offset = 2.4f;

        private void expand(double duration, Easing easing)
        {
            leftLayer.MoveToX(70, duration, easing);
            leftLayer.FadeColour(active_colour, duration, easing);
            icon.MoveToX(70 / centre_offset, duration, easing);
            rightLayer.MoveToX(right_layer_active + 10, duration, easing);
            text.MoveToX((right_layer_active - 60) / centre_offset + 70, duration, easing);
        }

        private void contract(double duration, Easing easing)
        {
            leftLayer.MoveToX(40, duration, easing);
            leftLayer.FadeColour(idle_colour, duration, easing);
            icon.MoveToX(40 / centre_offset, duration, easing);
            rightLayer.MoveToX(right_layer_idle + 10, duration, easing);
            text.MoveToX((right_layer_idle - 30) / centre_offset + 40, duration, easing);
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
