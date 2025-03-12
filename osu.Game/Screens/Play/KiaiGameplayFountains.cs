// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Menu;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play
{
    public partial class KiaiGameplayFountains : BeatSyncedContainer
    {
        private StarFountain leftFountain = null!;
        private StarFountain rightFountain = null!;

        private Bindable<bool> kiaiStarFountains = null!;

        private SkinnableSound? sample;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            kiaiStarFountains = config.GetBindable<bool>(OsuSetting.StarFountains);

            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
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
                sample = new SkinnableSound(new SampleInfo("Gameplay/fountain-shoot"))
            };
        }

        private bool isTriggered;

        protected override void Update()
        {
            base.Update();

            if (!kiaiStarFountains.Value)
                return;

            if (EffectPoint.KiaiMode && !isTriggered)
            {
                bool isNearEffectPoint = Math.Abs(BeatSyncSource.Clock.CurrentTime - EffectPoint.Time) < 500;
                if (isNearEffectPoint)
                    Shoot();
            }

            isTriggered = EffectPoint.KiaiMode;
        }

        public void Shoot()
        {
            leftFountain.Shoot(1);
            rightFountain.Shoot(-1);

            sample?.Play();
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
