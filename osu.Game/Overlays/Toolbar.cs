//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Threading;
using osu.Game;
using osu.Game.Configuration;
using osu.Game.GameModes.Play;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online;
using System.Collections.Generic;

namespace osu.Game.Overlays
{
    public class Toolbar : OverlayContainer
    {
        private const float height = 50;

        private Scheduler scheduler = new Scheduler();
        private APIAccess api;
        private User user;

        protected Avatar toolbarAvatar;
        protected BaseGame game;

        public Action OnSettings;
        public Action OnHome;
        public Action OnProfile;
        public Action<PlayMode> OnPlayModeChange;

        private ToolbarModeSelector modeSelector;

        private const int transition_time = 200;

        protected override void PopIn()
        {
            MoveToY(0, transition_time, EasingTypes.OutQuint);
            FadeIn(transition_time, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            MoveToY(-Size.Y, transition_time, EasingTypes.InQuint);
            FadeOut(transition_time, EasingTypes.InQuint);
        }

        protected void DrawToolbar()
        {
            FlowContainer rightSide;

            Add(rightSide = new FlowContainer
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Direction = FlowDirection.HorizontalOnly,
                RelativeSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new ToolbarButton
                    {
                        Icon = FontAwesome.search
                    },
                    new UserButton(user.UserId)
                    {
                        Image = toolbarAvatar,
                        Text = ((OsuGame)game).Config.Get<string>(OsuConfig.Username)
                    },
                    new ToolbarButton
                    {
                        Icon = FontAwesome.bars
                    },
                }
            });
        }

        private void SetUser()
        {
            MyUser req = new MyUser();
            req.Success += delegate (User MyUser)
            {
                this.user = MyUser;
            };

            api.Queue(req);
            
            api.Scheduler.Update();
            Update();
        }


        public override void Load(BaseGame game)
        {
            base.Load(game);

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
                            TooltipMain = "Settings",
                            TooltipSub = "Change your settings",
                            Action = OnSettings
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
                }
            };

            this.api = ((OsuGameBase)game).API;
            this.game = game;
            this.user = new User();
            ///scheduler.Add(delegate { SetUser(); });
            SetUser();
            this.toolbarAvatar = new Avatar(user.UserId, 48, 6);

            scheduler.AddDelayed(() => DrawToolbar(), 1000);

            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, height);
        }

        protected override void Update()
        {
            base.Update();
            api.Update();
            scheduler.Update();
        }

        
        public void SetGameMode(PlayMode mode) => modeSelector.SetGameMode(mode);
    }
}
