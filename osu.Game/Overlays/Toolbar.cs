//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Configuration;
using System;
using System.Linq;
using osu.Game.GameModes.Play;
using osu.Framework.Extensions;

namespace osu.Game.Overlays
{
    public partial class Toolbar : Container
    {
        const float height = 50;
        private FlowContainer leftFlow;
        private FlowContainer rightFlow;

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
                leftFlow = new FlowContainer
                {
                    Direction = FlowDirection.HorizontalOnly,
                    RelativeSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new ToolbarButton
                        {
                            Icon = FontAwesome.gear,
                            Action = OnSettings,
                            TooltipMain = "Settings"
                        },
                        new ToolbarButton
                        {
                            Icon = FontAwesome.home,
                            TooltipMain = "Home",
                            Action = OnHome
                        },
                        modeSelector = new ToolbarModeSelector
                        {
                            OnPlayModeChange = this.OnPlayModeChange
                        }
                    }
                },
                rightFlow = new FlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FlowDirection.HorizontalOnly,
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(0, 1),
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
