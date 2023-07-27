// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Menu
{
    public partial class KiaiMenuFountains : BeatSyncedContainer
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new[]
            {
                new StarFountain
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    X = 250,
                },
                new StarFountain
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    X = -250,
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

            foreach (var fountain in Children.OfType<StarFountain>())
                fountain.Shoot();

            lastTrigger = Clock.CurrentTime;
        }
    }
}
