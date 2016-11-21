//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Background;

namespace osu.Game.Screens.Backgrounds
{
    public class BackgroundModeBeatmap : BackgroundMode
    {
        private Background background;

        private WorkingBeatmap beatmap;

        public WorkingBeatmap Beatmap
        {
            get
            {
                return beatmap;
            }

            set
            {
                if (beatmap == value)
                    return;

                beatmap = value;

                Background oldBackground = background;

                addBackground(background = new Background());
                background.Sprite.Texture = beatmap.Background;

                if (oldBackground != null)
                {
                    oldBackground.Depth = 1;
                    oldBackground.Flush();
                    oldBackground.FadeOut(250);
                    oldBackground.Expire();

                    background.BlurSigma = oldBackground.BlurSigma;
                }
            }
        }

        public BackgroundModeBeatmap(WorkingBeatmap beatmap)
        {
            Beatmap = beatmap;
        }

        private void addBackground(Background background)
        {
            background.CacheDrawnFrameBuffer = true;
            Add(background);
        }

        public void BlurTo(Vector2 sigma, double duration)
        {
            background?.BlurTo(sigma, duration, EasingTypes.OutExpo);
        }

        public override bool Equals(BackgroundMode other)
        {
            return base.Equals(other) && beatmap == ((BackgroundModeBeatmap)other).Beatmap;
        }
    }
}
