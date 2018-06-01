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

        private readonly OsuSpriteText count, slash, maxText;

        public int Count
        {
            set => count.Text = value.ToString();
        }

        private int? max;
        public int? Max
        {
            get => max;
            set
            {
                if (value == max) return;
                max = value;

                updateMax();
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
                maxText = new OsuSpriteText
                {
                    TextSize = text_size,
                    Font = @"Exo2.0-Light"
                },
            };

            updateMax();
        }

        private void updateMax()
        {
            if (Max == null)
            {
                slash.FadeOut(transition_duration);
                maxText.FadeOut(transition_duration);
            }
            else
            {
                slash.FadeIn(transition_duration);
                maxText.Text = Max.ToString();
                maxText.FadeIn(transition_duration);
            }
        }
    }
}
