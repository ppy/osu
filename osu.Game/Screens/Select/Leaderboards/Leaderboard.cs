// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics;
using osu.Game.Modes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class Leaderboard : Container
    {
        private ScrollContainer scrollContainer;
        private FillFlowContainer<LeaderboardScoreDisplay> scrollFlow;

        private Score[] scores;
        public Score[] Scores
        {
            get { return scores; }
            set
            {
                if (value == scores) return;
                scores = value;

                var scoreDisplays = new List<LeaderboardScoreDisplay>();
                for (int i = 0; i < value.Length; i++)
                {
                    scoreDisplays.Add(new LeaderboardScoreDisplay(value[i], i + 1));
                }

                scrollFlow.Children = scoreDisplays;
                scrollContainer.ScrollTo(0f, false);
            }
        }

        public Leaderboard()
        {
            Children = new Drawable[]
            {
                scrollContainer = new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollDraggerVisible = false,
                    Children = new Drawable[]
                    {
                        scrollFlow = new FillFlowContainer<LeaderboardScoreDisplay>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(0f, 5f),
                        },
                    },
                },
            };
        }
    }

    class LeaderboardFade : Container
    {
        private Box gradient;

        protected override bool OnMouseDown(Framework.Input.InputState state, Framework.Input.MouseDownEventArgs args)
        {
            if (state.Keyboard.ShiftPressed)
                FadeIn();
            else
                FadeOut();
            
            return base.OnMouseDown(state, args);
        }

        public void FadeIn()
        {
            gradient.FadeColour(Color4.White, 300, EasingTypes.OutQuint);
        }

        public void FadeOut()
        {
            gradient.FadeColour(Color4.White.Opacity(0), 300, EasingTypes.OutQuint);
        }
    }
}
