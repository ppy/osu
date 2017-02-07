// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Graphics.Sprites;
using osu.Game.Modes;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Play
{
    class FailDialog : OsuGameMode
    {
        protected override BackgroundMode CreateBackground() => new BackgroundModeBeatmap(Beatmap);

        private static readonly Vector2 background_blur = new Vector2(20);

        public FailDialog()
        {
            Add(new OsuSpriteText
            {
                Text = "You failed!",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                TextSize = 50
            });
        }

        protected override void OnEntering(GameMode last)
        {
            base.OnEntering(last);
            Background.Schedule(() => (Background as BackgroundModeBeatmap)?.BlurTo(background_blur, 1000));
        }

        protected override bool OnExiting(GameMode next)
        {
            Background.Schedule(() => Background.FadeColour(Color4.White, 500));
            return base.OnExiting(next);
        }
    }
}
