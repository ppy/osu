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
using osu.Game.GameModes.Play;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online;

namespace osu.Game.Overlays
{
    public class Toolbar : OverlayContainer
    {
        private APIAccess api;
        private BaseGame game;
        private User user;

        public Action OnSettings;
        public Action OnHome;
        public Action OnProfile;
        public Action<PlayMode> OnPlayModeChange;

        private ToolbarModeSelector modeSelector;
        private UserButton toolbarUserButton;

        private const int transition_time = 200;
        private const float height = 50;

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

        private void CheckUser()
        {
            if (user == null)
                return;
            if (user.UserId == 0)
            {
                toolbarUserButton.UpdateButton(1, "Not signed in");
            }
            else if (user.UserId != toolbarUserButton.UserId)
            {
                toolbarUserButton.UpdateButton(user.UserId, user.Name);
                toolbarUserButton.UserId = user.UserId;
            }
        }

        private void InitUser()
        {
            MyUser req = new MyUser();
            api.Queue(req);
            req.Success += delegate (User MyUser)
            {
                this.user = MyUser;
                CheckUser();
            };
        }


        public override void Load(BaseGame game)
        {
            base.Load(game);
            api = ((OsuGameBase)game).API;
            this.game = game;
            user = new User();
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
                    AutoSizeAxes = Axes.X,
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
                },
                new FlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FlowDirection.HorizontalOnly,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new ToolbarButton
                        {
                            Icon = FontAwesome.search
                        },
                        toolbarUserButton = new UserButton(1),
                        new ToolbarButton
                        {
                            Icon = FontAwesome.bars
                        },
                    }
                }
            };

            InitUser();
            Scheduler.AddDelayed(delegate {
                InitUser();
            }, 10000, true);

            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, height);
        }

        protected override void Update()
        {
            base.Update();
            api.Update();
        }
        
        public void SetGameMode(PlayMode mode) => modeSelector.SetGameMode(mode);
    }
}