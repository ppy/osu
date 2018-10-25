// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Core.Screens.Evast;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using OpenTK;
using OpenTK.Input;

namespace osu.Mods.Evast.Galaga
{
    public class GalagaTestScreen : BeatmapScreen
    {
        private const int shootDelay = 200;

        private readonly BulletsContainer bulletsContainer;
        private readonly GalagaPlayer player;

        public GalagaTestScreen()
        {
            Children = new Drawable[]
            {
                bulletsContainer = new BulletsContainer(),
                player = new GalagaPlayer(bulletsContainer) { Position = new Vector2(0.1f, 0.5f) },
                new GalagaEnemy(bulletsContainer) { Position = new Vector2(0.8f, 0.2f) },
                new GalagaEnemy(bulletsContainer) { Position = new Vector2(0.8f, 0.5f) },
                new GalagaEnemy(bulletsContainer) { Position = new Vector2(0.8f, 0.8f) },
                new GalagaEnemy(bulletsContainer) { Position = new Vector2(0.7f, 0.2f) },
                new GalagaEnemy(bulletsContainer) { Position = new Vector2(0.7f, 0.5f) },
                new GalagaEnemy(bulletsContainer) { Position = new Vector2(0.7f, 0.8f) },
                new GalagaEnemy(bulletsContainer) { Position = new Vector2(0.6f, 0.2f) },
                new GalagaEnemy(bulletsContainer) { Position = new Vector2(0.6f, 0.5f) },
                new GalagaEnemy(bulletsContainer) { Position = new Vector2(0.6f, 0.8f) },
            };
        }

        private bool playerIsShooting;

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!playerIsShooting && e.Key == Key.Q)
            {
                shoot();
                playerIsShooting = true;
            }

            return base.OnKeyDown(e);
        }

        protected override bool OnKeyUp(KeyUpEvent e)
        {
            if (playerIsShooting && e.Key == Key.Q)
            {
                Scheduler.CancelDelayedTasks();
                playerIsShooting = false;
            }

            return base.OnKeyUp(e);
        }

        private void shoot()
        {
            player.Shoot();
            Scheduler.AddDelayed(shoot, shootDelay);
        }
    }
}
