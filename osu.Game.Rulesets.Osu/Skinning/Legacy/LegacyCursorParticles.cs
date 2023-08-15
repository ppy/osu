// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public partial class LegacyCursorParticles : CompositeDrawable, IKeyBindingHandler<OsuAction>
    {
        public bool Active => breakSpewer.Active.Value || kiaiSpewer.Active.Value || relaxHitSpewer.Active.Value;

        private LegacyCursorParticleSpewer breakSpewer = null!;
        private LegacyCursorParticleSpewer kiaiSpewer = null!;
        private LegacyRelaxCursorParticleSpewer relaxHitSpewer = null!;

        private readonly BindableBool relaxing = new BindableBool();
        private Bindable<bool> spawnParticlesOnHit = null!;

        [Resolved(canBeNull: true)]
        private Player? player { get; set; }

        [Resolved(canBeNull: true)]
        private OsuPlayfield? playfield { get; set; }

        [Resolved(canBeNull: true)]
        private GameplayState? gameplayState { get; set; }

        private ColourInfo starBreakAdditive = new Color4(255, 182, 193, 255);

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, OsuConfigManager osuConfig)
        {
            var texture = skin.GetTexture("star2");
            starBreakAdditive = skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.StarBreakAdditive)?.Value ?? new Color4(255, 182, 193, 255);

            if (texture != null)
            {
                // stable "magic ratio". see OsuPlayfieldAdjustmentContainer for full explanation.
                texture.ScaleAdjust *= 1.6f;
            }

            InternalChildren = new[]
            {
                breakSpewer = new LegacyCursorParticleSpewer(texture, 20)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = starBreakAdditive,
                    Direction = SpewDirection.None,
                },
                kiaiSpewer = new LegacyCursorParticleSpewer(texture, 60)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = starBreakAdditive,
                    Direction = SpewDirection.None,
                },
                relaxHitSpewer = new LegacyRelaxCursorParticleSpewer(texture, 60)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = starBreakAdditive,
                    Direction = SpewDirection.RelaxHit,
                }
            };

            if (player != null)
                ((IBindable<bool>)breakSpewer.Active).BindTo(player.IsBreakTime);

            relaxHitSpewer.Active.BindTo(relaxing);

            spawnParticlesOnHit = osuConfig.GetBindable<bool>(OsuSetting.SpawnParticlesOnHit);
            spawnParticlesOnHit.BindValueChanged(v => relaxing.Value = v.NewValue && player != null &&
                                                                       (player.Mods.Value.OfType<OsuModRelax>().Any() ||
                                                                        player.Mods.Value.OfType<OsuModAutopilot>().Any()), true);
        }

        protected override void Update()
        {
            if (playfield == null || gameplayState == null) return;

            DrawableHitObject? kiaiHitObject = null;

            // Check whether currently in a kiai section first. This is only done as an optimisation to avoid enumerating AliveObjects when not necessary.
            if (gameplayState.Beatmap.ControlPointInfo.EffectPointAt(Time.Current).KiaiMode)
                kiaiHitObject = playfield.HitObjectContainer.AliveObjects.FirstOrDefault(isTracking);

            kiaiSpewer.Active.Value = kiaiHitObject != null || relaxingKeyPressed();
        }

        private bool doRelaxingSpew()
        {
            if (playfield == null) return false;

            bool hitting = playfield.HitObjectContainer.AliveObjects.FirstOrDefault(isHit) != null;

            return hitting && relaxingKeyPressed() && !breakSpewer.Active.Value;
        }

        private bool relaxingKeyPressed() => relaxing.Value && (leftPressed || rightPressed);

        private bool isHit(DrawableHitObject h)
        {
            switch (h)
            {
                case DrawableSlider slider:
                    return slider.HeadCircle.IsHit;

                case DrawableHitCircle hitCircle:
                    return hitCircle.IsHit;
            }

            return false;
        }

        private bool isTracking(DrawableHitObject h)
        {
            if (!h.HitObject.Kiai)
                return false;

            switch (h)
            {
                case DrawableSlider slider:
                    return slider.Tracking.Value;

                case DrawableSpinner spinner:
                    return spinner.RotationTracker.Tracking;
            }

            return false;
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

            if (leftPressed && rightPressed)
                breakSpewer.Direction = SpewDirection.Omni;
            else if (leftPressed)
                breakSpewer.Direction = SpewDirection.Left;
            else if (rightPressed)
                breakSpewer.Direction = SpewDirection.Right;
            else
                breakSpewer.Direction = SpewDirection.None;

            if (pressed && action is OsuAction.LeftButton or OsuAction.RightButton && doRelaxingSpew())
                spawnRelaxHitParticles();
        }

        private void spawnRelaxHitParticles()
        {
            if (playfield == null) return;

            relaxHitSpewer.Direction = SpewDirection.RelaxHit;

            var firstHitObject = playfield.HitObjectContainer.AliveObjects.FirstOrDefault();

            switch (firstHitObject)
            {
                case DrawableSlider slider:
                    relaxHitSpewer.Colour = slider.HeadCircle.AccentColour.Value;
                    break;

                case DrawableHitCircle hitCircle:
                    relaxHitSpewer.Colour = hitCircle.AccentColour.Value;
                    break;
            }

            relaxHitSpewer.SpawnRelaxingHitParticles();
        }

        private partial class LegacyCursorParticleSpewer : ParticleSpewer, IRequireHighFrequencyMousePosition
        {
            private const int particle_duration_min = 300;
            private const int particle_duration_max = 1000;

            public SpewDirection Direction { get; set; }

            protected override bool CanSpawnParticles => base.CanSpawnParticles && cursorScreenPosition.HasValue;
            protected override float ParticleGravity => 240;

            public LegacyCursorParticleSpewer(Texture? texture, int perSecond)
                : base(texture, perSecond, particle_duration_max)
            {
                Active.BindValueChanged(_ => resetVelocityCalculation());
            }

            private Vector2? cursorScreenPosition;
            private Vector2 cursorVelocity;

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

            private void resetVelocityCalculation()
            {
                cursorScreenPosition = null;
                totalPosDifference = Vector2.Zero;
                velocityFrameLength = 0;
            }

            protected override FallingParticle CreateParticle() =>
                new FallingParticle
                {
                    StartPosition = ToLocalSpace(cursorScreenPosition ?? Vector2.Zero),
                    Duration = RNG.NextSingle(particle_duration_min, particle_duration_max),
                    StartAngle = (float)(RNG.NextDouble() * 4 - 2),
                    EndAngle = RNG.NextSingle(-2f, 2f),
                    EndScale = RNG.NextSingle(2f),
                    Velocity = getVelocity(),
                    Colour = Colour,
                };

            private Vector2 getVelocity()
            {
                Vector2 velocity = Vector2.Zero;

                switch (Direction)
                {
                    case SpewDirection.Left:
                        velocity = new Vector2(
                            RNG.NextSingle(-460f, 0),
                            RNG.NextSingle(-40f, 40f)
                        );
                        break;

                    case SpewDirection.Right:
                        velocity = new Vector2(
                            RNG.NextSingle(0, 460f),
                            RNG.NextSingle(-40f, 40f)
                        );
                        break;

                    case SpewDirection.Omni:
                        velocity = new Vector2(
                            RNG.NextSingle(-460f, 460f),
                            RNG.NextSingle(-160f, 160f)
                        );
                        break;

                    case SpewDirection.RelaxHit:
                        velocity = new Vector2(
                            RNG.NextSingle(-500f, 500f),
                            RNG.NextSingle(-500f, 500f)
                        );
                        break;
                }

                velocity += cursorVelocity * 40;

                return velocity;
            }
        }

        private enum SpewDirection
        {
            None,
            Left,
            Right,
            Omni,
            RelaxHit,
        }

        private partial class LegacyRelaxCursorParticleSpewer : LegacyCursorParticleSpewer
        {
            // Disable spawn in Update()
            protected override bool CanSpawnParticles => false;

            public LegacyRelaxCursorParticleSpewer(Texture? texture, int perSecond)
                : base(texture, perSecond)
            {
            }

            public void SpawnRelaxingHitParticles()
            {
                if (!base.CanSpawnParticles || !Active.Value) return;

                for (int i = 0; i < 6; i++)
                    SpawnParticle();
            }
        }
    }
}
