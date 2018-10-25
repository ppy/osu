using System;
using osu.Core;
using osu.Core.Config;
using osu.Core.Screens.Evast;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osu.Game.Overlays.Settings;
using osu.Mods.Multi.Networking;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Mods.Multi.Screens
{
    public class ConnectToServer : BeatmapScreen
    {
        protected OsuNetworkingHandler OsuNetworkingHandler;

        public readonly SettingsButton HostGameButton;
        public readonly SettingsButton JoinGameButton;

        public readonly Container NewGame;

        protected readonly TextBox PortBox;
        protected readonly TextBox IpBox;

        private readonly Bindable<string> ipBindable = SymcolOsuModSet.SymcolConfigManager.GetBindable<string>(SymcolSetting.SavedIP);
        private readonly Bindable<int> portBindable = SymcolOsuModSet.SymcolConfigManager.GetBindable<int>(SymcolSetting.SavedPort);

        public ConnectToServer()
        {
            AlwaysPresent = true;
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                HostGameButton = new SettingsButton
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(12, -12),
                    RelativeSizeAxes = Axes.X,
                    Width = 0.3f,
                    Text = "Host Server",
                    Action = HostGame
                },
                JoinGameButton = new SettingsButton
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Position = new Vector2(-12, -12),
                    RelativeSizeAxes = Axes.X,
                    Width = 0.3f,
                    Text = "Connect To Server",
                    Action = JoinServer
                },
                NewGame = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,

                    Position = new Vector2(0, -40),
                    Size = new Vector2(400, 60),

                    CornerRadius = 10,
                    BorderColour = Color4.White,
                    BorderThickness = 6,

                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = ColourInfo.GradientVertical(Color4.DarkGreen, Color4.Green),
                            RelativeSizeAxes = Axes.Both
                        },
                        PortBox = new TextBox
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Position = new Vector2(-12, 0),
                            RelativeSizeAxes = Axes.X,
                            Width = 0.42f,
                            Height = 20,
                            PlaceholderText = "Port",
                        },
                        IpBox = new TextBox
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Position = new Vector2(12, 0),
                            RelativeSizeAxes = Axes.X,
                            Width = 0.42f,
                            Height = 20,
                            PlaceholderText = "IP"
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            IpBox.Text = ipBindable;
            PortBox.Text = portBindable.Value.ToString();
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);

            if (OsuNetworkingHandler != null)
            {
                Remove(OsuNetworkingHandler);
                OsuNetworkingHandler.Dispose();
                OsuNetworkingHandler = null;
            }
        }

        protected override bool OnExiting(Screen next)
        {
            if (OsuNetworkingHandler != null)
            {
                Remove(OsuNetworkingHandler);
                OsuNetworkingHandler.Dispose();
            }

            return base.OnExiting(next);
        }

        protected virtual void JoinServer()
        {
            if (OsuNetworkingHandler == null)
            {
                Add(OsuNetworkingHandler = new OsuNetworkingHandler
                {
                    Address = IpBox.Text + ":" + PortBox.Text
                });
                OsuNetworkingHandler.OnConnectedToHost += host => Connected();
            }
            OsuNetworkingHandler.Connect();
        }

        protected virtual void HostGame()
        {
            if (OsuNetworkingHandler == null)
            {
                Add(OsuNetworkingHandler = new OsuNetworkingHandler
                {
                    Address = IpBox.Text + ":" + PortBox.Text
                });
                OsuNetworkingHandler.OnConnectedToHost += host => Connected();
                OsuNetworkingHandler.Add(new OsuServerNetworkingHandler
                {
                    Address = IpBox.Text + ":" + PortBox.Text
                });
            }
            OsuNetworkingHandler.Connect();
        }

        protected virtual void Connected()
        {
            Remove(OsuNetworkingHandler);
            Push(new Lobby(OsuNetworkingHandler));
        }

        protected override void Dispose(bool isDisposing)
        {
            ipBindable.Value = IpBox.Text;

            try { portBindable.Value = Int32.Parse(PortBox.Text); }
            catch { portBindable.Value = 25590; }

            base.Dispose(isDisposing);
        }
    }
}

