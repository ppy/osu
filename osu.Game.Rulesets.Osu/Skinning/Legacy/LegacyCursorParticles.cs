// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Particles;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacyCursorParticles : CompositeDrawable, IKeyBindingHandler<OsuAction>
    {
        public bool Active => breakSpewer?.Active.Value == true || kiaiSpewer?.Active.Value == true;

        private LegacyCursorParticleSpewer breakSpewer;
        private LegacyCursorParticleSpewer kiaiSpewer;

        [Resolved(canBeNull: true)]
        private Player player { get; set; }

        [Resolved(canBeNull: true)]
        private OsuPlayfield osuPlayfield { get; set; }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, OsuColour colours)
        {
            var texture = skin.GetTexture("star2");
            if (texture == null)
                return;

            texture.ScaleAdjust = 3.2f;

            InternalChildren = new[]
            {
                breakSpewer = new LegacyCursorParticleSpewer(texture, 20)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = colours.PinkLighter,
                    Direction = SpewDirection.None,
                    Active =
                    {
                        Value = false,
                    }
                },
                kiaiSpewer = new LegacyCursorParticleSpewer(texture, 60)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = colours.PinkLighter,
                    Direction = SpewDirection.None,
                    Active =
                    {
                        Value = false,
                    }
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

        public bool OnPressed(OsuAction action)
        {
            handleInput(action, true);
            return false;
        }

        public void OnReleased(OsuAction action)
        {
            handleInput(action, false);
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

            if (leftPressed && rightPressed)
                breakSpewer.Direction = SpewDirection.Omni;
            else if (leftPressed)
                breakSpewer.Direction = SpewDirection.Left;
            else if (rightPressed)
                breakSpewer.Direction = SpewDirection.Right;
            else
                breakSpewer.Direction = SpewDirection.None;
        }

        private class LegacyCursorParticleSpewer : ParticleSpewer, IRequireHighFrequencyMousePosition
        {
            private const int particle_lifetime_min = 300;
            private const int particle_lifetime_max = 1000;

            public SpewDirection Direction { get; set; }

            protected override bool CanSpawnParticles => base.CanSpawnParticles && cursorScreenPosition.HasValue;
            protected override float ParticleGravity => 240;

            public LegacyCursorParticleSpewer(Texture texture, int perSecond)
                : base(texture, perSecond, particle_lifetime_max)
            {
                Active.BindValueChanged(_ => resetVelocityCalculation());
            }

            private Vector2? cursorScreenPosition;
            private Vector2 cursorVelocity;

            private const double max_velocity_frame_length = 15;
            private double velocityFrameLength;
            private Vector2 totalPosDifference;

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

                velocityFrameLength += Clock.ElapsedFrameTime;

                if (velocityFrameLength > max_velocity_frame_length)
                {
                    cursorVelocity = totalPosDifference / (float)velocityFrameLength;

                    totalPosDifference = Vector2.Zero;
                    velocityFrameLength = 0;
                }

                return base.OnMouseMove(e);
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            private void resetVelocityCalculation()
            {
                cursorScreenPosition = null;
                totalPosDifference = Vector2.Zero;
                velocityFrameLength = 0;
            }

            protected override FallingParticle SpawnParticle()
            {
                var p = base.SpawnParticle();

                p.StartPosition = ToLocalSpace(cursorScreenPosition ?? Vector2.Zero);
                p.Duration = RNG.NextSingle(particle_lifetime_min, particle_lifetime_max);
                p.StartAngle = (float)(RNG.NextDouble() * 4 - 2);
                p.EndAngle = RNG.NextSingle(-2f, 2f);
                p.EndScale = RNG.NextSingle(2f);

                switch (Direction)
                {
                    case SpewDirection.None:
                        p.Velocity = Vector2.Zero;
                        break;

                    case SpewDirection.Left:
                        p.Velocity = new Vector2(
                            RNG.NextSingle(-460f, 0),
                            RNG.NextSingle(-40f, 40f)
                        );
                        break;

                    case SpewDirection.Right:
                        p.Velocity = new Vector2(
                            RNG.NextSingle(0, 460f),
                            RNG.NextSingle(-40f, 40f)
                        );
                        break;

                    case SpewDirection.Omni:
                        p.Velocity = new Vector2(
                            RNG.NextSingle(-460f, 460f),
                            RNG.NextSingle(-160f, 160f)
                        );
                        break;
                }

                p.Velocity += cursorVelocity * 40;

                return p;
            }
        }

        private enum SpewDirection
        {
            None,
            Left,
            Right,
            Omni,
        }
    }
}
