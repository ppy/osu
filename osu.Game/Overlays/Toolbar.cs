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
    public class Toolbar : Container
    {
        const float height = 50;
        private FlowContainer leftFlow;
        private FlowContainer rightFlow;
        private FlowContainer modeButtons;

        public Action OnSettings;
        public Action OnHome;
        public Action<PlayMode> OnPlayModeChange;


        public override void Load()
        {
            base.Load();

            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, height);

            modeButtons = new FlowContainer
            {
                RelativeSizeAxes = Axes.Y,
                Direction = FlowDirection.HorizontalOnly
            };

            foreach (PlayMode m in Enum.GetValues(typeof(PlayMode)))
            {
                var localMode = m;
                modeButtons.Add(new ToolbarModeButton
                {
                    Mode = m,
                    Action = delegate
                    {
                        SetGameMode(localMode);
                        OnPlayModeChange?.Invoke(localMode);
                    }
                });
            }


            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(0.1f, 0.1f, 0.1f, 0.9f)
                },
                leftFlow = new FlowContainer
                {
                    Direction = FlowDirection.HorizontalOnly,
                    RelativeSizeAxes = Axes.Y,
                    Children = new []
                    {
                        new ToolbarButton
                        {
                            Icon = FontAwesome.gear,
                            Action = OnSettings
                        },
                        new ToolbarButton
                        {
                            Icon = FontAwesome.home,
                            Action = OnHome
                        },
                        modeButtons
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

        public void SetGameMode(PlayMode mode)
        {
            foreach (var m in modeButtons.Children.Cast<ToolbarModeButton>())
            {
                m.Active = m.Mode == mode;
            }
        }

        public class ToolbarModeButton : ToolbarButton
        {
            private PlayMode mode;
            public PlayMode Mode
            {
                get { return mode; }
                set
                {
                    mode = value;
                    Text = mode.GetDescription();
                    Icon = getModeIcon(mode);
                }
            }

            public bool Active
            {
                set
                {
                    Background.Colour = value ? new Color4(100, 100, 100, 140) : new Color4(20, 20, 20, 140);
                }
            }

            private FontAwesome getModeIcon(PlayMode mode)
            {
                switch (mode)
                {
                    default: return FontAwesome.fa_osu_osu_o;
                    case PlayMode.Taiko: return FontAwesome.fa_osu_taiko_o;
                    case PlayMode.Catch: return FontAwesome.fa_osu_fruits_o;
                    case PlayMode.Mania: return FontAwesome.fa_osu_mania_o;
                }
            }

            public override void Load()
            {
                base.Load();
                DrawableIcon.TextSize = height * 0.7f;
            }
        }

        public class ToolbarButton : FlowContainer
        {
            public FontAwesome Icon
            {
                get { return DrawableIcon.Icon; }
                set { DrawableIcon.Icon = value; }
            }

            public string Text
            {
                get { return DrawableText.Text; }
                set
                {
                    DrawableText.Text = value;
                    paddingIcon.Alpha = string.IsNullOrEmpty(value) ? 0 : 1;
                }
            }

            public Action Action;

            protected TextAwesome DrawableIcon;
            protected SpriteText DrawableText;
            protected Box Background;
            protected Box HoverBackground;
            private Drawable paddingLeft;
            private Drawable paddingRight;
            private Drawable paddingIcon;

            public new float Padding
            {
                get { return paddingLeft.Size.X; }
                set
                {
                    paddingLeft.Size = new Vector2(value, 1);
                    paddingRight.Size = new Vector2(value, 1);
                }
            }

            public ToolbarButton()
            {
                Background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(20, 20, 20, 140),
                };

                HoverBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Additive = true,
                    Colour = new Color4(20, 20, 20, 0),
                    Alpha = 0,
                };

                DrawableIcon = new TextAwesome()
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                };

                DrawableText = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                };

                paddingLeft = new Container { RelativeSizeAxes = Axes.Y };
                paddingRight = new Container { RelativeSizeAxes = Axes.Y };
                paddingIcon = new Container
                {
                    Size = new Vector2(5, 0),
                    Alpha = 0
                };

                Padding = 10;
            }

            protected override bool OnClick(InputState state)
            {
                Action?.Invoke();
                return base.OnClick(state);
            }

            protected override bool OnHover(InputState state)
            {
                HoverBackground.FadeTo(0.4f, 200);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                HoverBackground.FadeTo(0, 200);
                base.OnHoverLost(state);
            }

            public override void Load()
            {
                base.Load();

                RelativeSizeAxes = Axes.Y;
                Direction = FlowDirection.HorizontalOnly;

                Children = new Drawable[]
                {
                    Background,
                    HoverBackground,
                    paddingLeft,
                    DrawableIcon,
                    paddingIcon,
                    DrawableText,
                    paddingRight,
                };
            }
        }
    }
}
