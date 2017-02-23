// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Graphics.Sprites;
using osu.Game.Modes;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Overlays.Pause;
using System;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Screens.Play
{
    class FailDialog : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        private static readonly Vector2 background_blur = new Vector2(20);

        private PauseOverlay failOverlay;

        [BackgroundDependencyLoader]
        private void load()
        {
            failOverlay = new PauseOverlay()
            {
                Type = false,
                MainText = @"failed",
                AdditionalText = @"retry?",
                OnRetry = retry,
                OnQuit = Exit
            };
            Children = new Drawable[]
            {
                failOverlay
            };
            failOverlay.ToggleVisibility();
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            Background.Schedule(() => (Background as BackgroundScreenBeatmap)?.BlurTo(background_blur, 1000));
        }

        protected override bool OnExiting(Screen next)
        {
            Background.Schedule(() => Background.FadeColour(Color4.White, 500));
            return base.OnExiting(next);
        }

        private void retry()
        {
            var newPlayer = new Player();
            ValidForResume = false;

            newPlayer.Preload(Game, delegate
            {
                if (!Push(newPlayer))
                {
                    // Error(?)
                }
            });
        }

        public FailDialog()
        {
            RelativeSizeAxes = Axes.Both;
        }
    }
}
