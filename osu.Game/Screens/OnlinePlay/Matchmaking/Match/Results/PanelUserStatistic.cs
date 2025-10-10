// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.Results
{
    public partial class PanelUserStatistic : CompositeDrawable
    {
        private readonly int position;
        private readonly string text;

        public PanelUserStatistic(int position, string text)
        {
            this.position = position;
            this.text = text;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new CircularContainer
            {
                AutoSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(5, 0),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Font = OsuFont.Default.With(weight: FontWeight.Bold),
                                Text = position.Ordinalize(),
                            },
                            new OsuSpriteText
                            {
                                Text = text
                            }
                        }
                    },
                }
            };
        }
    }
}
