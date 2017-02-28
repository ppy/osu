// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Play
{
    class FailDialog : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

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
    }
}
