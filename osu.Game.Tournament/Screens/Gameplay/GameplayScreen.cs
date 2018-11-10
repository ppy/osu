// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Screens.Gameplay
{
    public class GameplayScreen : BeatmapInfoScreen
    {
        private readonly BindableBool warmup = new BindableBool();

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, TextureStore textures)
        {
            AddRange(new Drawable[]
            {
                new MatchHeader(),
                // new CustomChatOverlay
                // {
                //     Anchor = Anchor.BottomCentre,
                //     Origin = Anchor.BottomCentre,
                //     Size = new Vector2(0.4f, 1)
                // },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new TriangleButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Toggle warmup",
                            Action = toggleWarmup
                        }
                    }
                }
            });
        }

        private void toggleWarmup()
        {
            warmup.Toggle();
        }
    }
}
