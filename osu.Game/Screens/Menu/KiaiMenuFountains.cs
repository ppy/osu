// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Graphics.Containers;
using osu.Game.Skinning;

namespace osu.Game.Screens.Menu
{
    public partial class KiaiMenuFountains : BeatSyncedContainer
    {
        private StarFountain leftFountain = null!;
        private StarFountain rightFountain = null!;

        [Resolved]
        private GameHost host { get; set; } = null!;

        private SkinnableSound? sample;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                leftFountain = new StarFountain
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    X = 250,
                },
                rightFountain = new StarFountain
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    X = -250,
                },
                sample = new SkinnableSound(new SampleInfo("Gameplay/fountain-shoot"))
            };
        }

        private bool isTriggered;

        protected override void Update()
        {
            base.Update();

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
            int direction = RNG.Next(-1, 2);

            switch (direction)
            {
                case -1:
                    leftFountain.Shoot(1);
                    rightFountain.Shoot(-1);
                    break;

                case 0:
                    leftFountain.Shoot(0);
                    rightFountain.Shoot(0);
                    break;

                case 1:
                    leftFountain.Shoot(-1);
                    rightFountain.Shoot(1);
                    break;
            }

            // Don't play SFX when game is in background as it can be a bit noisy.
            if (host.IsActive.Value)
                sample?.Play();
        }
    }
}
