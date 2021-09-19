// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacyCursorParticles : CompositeDrawable, IKeyBindingHandler<OsuAction>, IRequireHighFrequencyMousePosition
    {
        private const int particle_lifetime_min = 300;
        private const int particle_lifetime_max = 1000;
        private const float particle_gravity = 240;

        public bool Active => breakSpewer?.Active.Value == true || kiaiSpewer?.Active.Value == true;

        private Vector2 cursorVelocity;
        private ParticleSpewer breakSpewer;
        private ParticleSpewer kiaiSpewer;

        [Resolved(canBeNull: true)]
        private Player player { get; set; }

        [Resolved(canBeNull: true)]
        private OsuPlayfield osuPlayfield { get; set; }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, OsuColour colours)
        {
            var texture = skin.GetTexture("star2");
            var starBreakAdditive = skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.StarBreakAdditive)?.Value ?? new Color4(255, 182, 193, 255);

            if (texture != null)
            {
                // stable "magic ratio". see OsuPlayfieldAdjustmentContainer for full explanation.
                texture.ScaleAdjust *= 1.6f;
            }

            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            InternalChildren = new[]
            {
                breakSpewer = new ParticleSpewer(texture, 20, particle_lifetime_max)
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One,
                    Colour = starBreakAdditive,
                    ParticleGravity = particle_gravity,
                    CreateParticle = createBreakParticle,
                },
                kiaiSpewer = new ParticleSpewer(texture, 60, particle_lifetime_max)
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One,
                    Colour = starBreakAdditive,
                    ParticleGravity = particle_gravity,
                    CreateParticle = createParticle,
                },
            };

            if (player != null)
            {
                breakSpewer.Active.BindTarget = player.IsBreakTime;
            }
        }

        protected override void Update()
        {
            if (osuPlayfield == null) return;

            // find active kiai slider or spinner.
            var kiaiHitObject = osuPlayfield.HitObjectContainer.AliveObjects.FirstOrDefault(h =>
                h.HitObject.Kiai &&
                (
                    (h is DrawableSlider slider && slider.Tracking.Value) ||
                    (h is DrawableSpinner spinner && spinner.RotationTracker.Tracking)
                )
            );

            kiaiSpewer.Active.Value = kiaiHitObject != null;
        }

        private Vector2? cursorScreenPosition;

        private const double max_velocity_frame_length = 15;
        private double velocityFrameLength;
        private Vector2 totalPosDifference;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (cursorScreenPosition == null)
            {
                cursorScreenPosition = e.ScreenSpaceMousePosition;
                return base.OnMouseMove(e);
            }

            // calculate cursor velocity.
            totalPosDifference += e.ScreenSpaceMousePosition - cursorScreenPosition.Value;
            cursorScreenPosition = e.ScreenSpaceMousePosition;

            velocityFrameLength += Math.Abs(Clock.ElapsedFrameTime);

            if (velocityFrameLength > max_velocity_frame_length)
            {
                cursorVelocity = totalPosDifference / (float)velocityFrameLength;

                totalPosDifference = Vector2.Zero;
                velocityFrameLength = 0;
            }

            return base.OnMouseMove(e);
        }

        public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
        {
            handleInput(e.Action, true);
            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
        {
            handleInput(e.Action, false);
        }

        private bool leftPressed;
        private bool rightPressed;

        private void handleInput(OsuAction action, bool pressed)
        {
            switch (action)
            {
                case OsuAction.LeftButton:
                    leftPressed = pressed;
                    break;

                case OsuAction.RightButton:
                    rightPressed = pressed;
                    break;
            }
        }

        private ParticleSpewer.FallingParticle? createParticle()
        {
            if (!cursorScreenPosition.HasValue) return null;

            return new ParticleSpewer.FallingParticle
            {
                StartPosition = ToLocalSpace(cursorScreenPosition.Value),
                Duration = RNG.NextSingle(particle_lifetime_min, particle_lifetime_max),
                StartAngle = (float)(RNG.NextDouble() * 4 - 2),
                EndAngle = RNG.NextSingle(-2f, 2f),
                EndScale = RNG.NextSingle(2f),
                Velocity = cursorVelocity * 40,
            };
        }

        private ParticleSpewer.FallingParticle? createBreakParticle()
        {
            var baseParticle = createParticle();
            if (!baseParticle.HasValue) return null;

            var p = baseParticle.Value;

            if (leftPressed && rightPressed)
            {
                p.Velocity += new Vector2(
                    RNG.NextSingle(-460f, 460f),
                    RNG.NextSingle(-160f, 160f)
                );
            }
            else if (leftPressed)
            {
                p.Velocity += new Vector2(
                    RNG.NextSingle(-460f, 0),
                    RNG.NextSingle(-40f, 40f)
                );
            }
            else if (rightPressed)
            {
                p.Velocity += new Vector2(
                    RNG.NextSingle(0, 460f),
                    RNG.NextSingle(-40f, 40f)
                );
            }

            return p;
        }
    }
}
