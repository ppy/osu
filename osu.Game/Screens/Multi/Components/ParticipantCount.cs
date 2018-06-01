// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Multi.Components
{
    public class ParticipantCount : FillFlowContainer
    {
        private const float text_size = 30;
        private const float transition_duration = 100;

        private readonly OsuSpriteText count, slash, max;

        public int Count
        {
            set => count.Text = value.ToString();
        }

        public int? Max
        {
            set
            {
                if (value == null)
                {
                    slash.FadeOut(transition_duration);
                    max.FadeOut(transition_duration);
                }
                else
                {
                    slash.FadeIn(transition_duration);
                    max.Text = value.ToString();
                    max.FadeIn(transition_duration);
                }
            }
        }

        public ParticipantCount()
        {
            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Horizontal;
            LayoutDuration = transition_duration;

            Children = new[]
            {
                count = new OsuSpriteText
                {
                    TextSize = text_size,
                    Font = @"Exo2.0-Bold"
                },
                slash = new OsuSpriteText
                {
                    Text = @"/",
                    TextSize = text_size,
                    Font = @"Exo2.0-Light"
                },
                max = new OsuSpriteText
                {
                    TextSize = text_size,
                    Font = @"Exo2.0-Light"
                },
            };

            Max = null;
        }
    }
}
