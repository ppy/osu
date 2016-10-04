//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Game.Configuration;
using System;
using osu.Game.GameModes.Play;

namespace osu.Game.Overlays
{
    public class Toolbar : Container
    {
        const float height = 50;

        public Action OnSettings;
        public Action OnHome;
        public Action<PlayMode> OnPlayModeChange;

        private ToolbarModeSelector modeSelector;

        public override void Load()
        {
            base.Load();

            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, height);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(0.1f, 0.1f, 0.1f, 0.4f)
                },
                new FlowContainer
                {
                    Direction = FlowDirection.HorizontalOnly,
                    RelativeSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new ToolbarButton
                        {
                            Icon = FontAwesome.gear,
                            Action = OnSettings,
                            TooltipMain = "Settings",
                            TooltipSub = "Change your settings",
                        },
                        new ToolbarButton
                        {
                            Icon = FontAwesome.home,
                            TooltipMain = "Home",
                            TooltipSub = "Return to the main menu",
                            Action = OnHome
                        },
                        modeSelector = new ToolbarModeSelector
                        {
                            OnPlayModeChange = OnPlayModeChange
                        }
                    }
                },
                new FlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FlowDirection.HorizontalOnly,
                    RelativeSizeAxes = Axes.Y,
                    Children = new []
                    {
                        new ToolbarButton
                        {
                            Icon = FontAwesome.search
                        },
                        new ToolbarButton
                        {
                            Icon = FontAwesome.user,
                            Text = ((OsuGame)Game).Config.Get<string>(OsuConfig.Username)
                        },
                        new ToolbarButton
                        {
                            Icon = FontAwesome.bars
                        },
                    }
                }
            };
        }

        public void SetGameMode(PlayMode mode) => modeSelector.SetGameMode(mode);
    }
}
