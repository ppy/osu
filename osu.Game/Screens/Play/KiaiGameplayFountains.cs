// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Menu;

namespace osu.Game.Screens.Play
{
    public partial class KiaiGameplayFountains : BeatSyncedContainer
    {
        private StarFountain leftFountain = null!;
        private StarFountain rightFountain = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new[]
            {
                leftFountain = new GameplayStarFountain
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    X = 75,
                },
                rightFountain = new GameplayStarFountain
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    X = -75,
                },
            };
        }

        private bool isTriggered;

        private double? lastTrigger;

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            if (effectPoint.KiaiMode && !isTriggered)
            {
                bool isNearEffectPoint = Math.Abs(BeatSyncSource.Clock.CurrentTime - effectPoint.Time) < 500;
                if (isNearEffectPoint)
                    Shoot();
            }

            isTriggered = effectPoint.KiaiMode;
        }

        public void Shoot()
        {
            if (lastTrigger != null && Clock.CurrentTime - lastTrigger < 500)
                return;

            leftFountain.Shoot(1);
            rightFountain.Shoot(-1);
            lastTrigger = Clock.CurrentTime;
        }

        public partial class GameplayStarFountain : StarFountain
        {
            protected override StarFountainSpewer CreateSpewer() => new GameplayStarFountainSpewer();

            private partial class GameplayStarFountainSpewer : StarFountainSpewer
            {
                protected override double ShootDuration => 400;

                public GameplayStarFountainSpewer()
                    : base(perSecond: 180)
                {
                }

                protected override float GetCurrentAngle()
                {
                    const float x_velocity_from_direction = 450;
                    const float x_velocity_to_direction = 600;

                    return LastShootDirection * RNG.NextSingle(x_velocity_from_direction, x_velocity_to_direction);
                }
            }
        }
    }
}
