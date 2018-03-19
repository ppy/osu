using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Screens;
using System;
using osu.Framework.Screens;
using System.Collections.Generic;
using Symcol.Core.Networking;
using Symcol.Rulesets.Core.Multiplayer.Networking;

namespace Symcol.Rulesets.Core.Multiplayer.Screens
{
    public abstract class RulesetLobbyScreen : OsuScreen
    {
        public abstract string RulesetName { get; }

        public abstract RulesetMatchScreen MatchScreen { get; }

        public RulesetNetworkingClientHandler RulesetNetworkingClientHandler;

        public readonly SettingsButton HostGameButton;
        public readonly SettingsButton DirectConnectButton;
        public readonly SettingsButton JoinGameButton;

        public readonly Container NewGame;
        protected readonly TextBox HostIP;
        protected readonly TextBox HostPort;
        //protected readonly TextBox PublicIp;
        protected readonly TextBox LocalIp;

        public readonly Container JoinIP;

        public RulesetLobbyScreen()
        {
            AlwaysPresent = true;
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                HostGameButton = new SettingsButton
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.3f,
                    Text = "Host Game",
                    Action = HostGame
                },
                DirectConnectButton = new SettingsButton
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.3f,
                    Text = "Direct Connect",
                    Action = DirectConnect
                },
                JoinGameButton = new SettingsButton
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.3f,
                    Text = "Join Game"
                },
                NewGame = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    Size = new Vector2(400, 300),

                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Blue,
                            RelativeSizeAxes = Axes.Both
                        },
                        new Box
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.Black,
                            Alpha = 0.9f,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.48f,
                            Height = 20,
                        },
                        HostIP = new TextBox
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.48f,
                            Height = 20,
                            Text = "Host IP Address"
                        },
                        new Box
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Colour = Color4.Black,
                            Alpha = 0.9f,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.48f,
                            Height = 20,
                        },
                        HostPort = new TextBox
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.48f,
                            Height = 20,
                            Text = "25570"
                        },
                        /*
                        new Box
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.Black,
                            Alpha = 0.9f,
                            RelativeSizeAxes = Axes.X,
                            Position = new Vector2(0, 22),
                            Width = 0.48f,
                            Height = 20,
                        },
                        PublicIp = new TextBox
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.X,
                            Position = new Vector2(0, 22),
                            Width = 0.48f,
                            Height = 20,
                            Text = "You're Public IP Address"
                        },
                        */
                        new Box
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.Black,
                            Alpha = 0.9f,
                            RelativeSizeAxes = Axes.X,
                            Position = new Vector2(0, 44),
                            Width = 0.48f,
                            Height = 20,
                        },
                        LocalIp = new TextBox
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.X,
                            Position = new Vector2(0, 44),
                            Width = 0.48f,
                            Height = 20,
                            Text = "You're Local IP Address"
                        }
                    }
                }
            };
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            MakeCurrent();
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);
            MakeCurrent();
        }

        protected override bool OnExiting(Screen next)
        {
            if (RulesetNetworkingClientHandler != null)
            {
                Remove(RulesetNetworkingClientHandler);
                RulesetNetworkingClientHandler.Dispose();
            }

            return base.OnExiting(next);
        }

        protected virtual void HostGame()
        {
            if (RulesetNetworkingClientHandler != null)
            {
                Remove(RulesetNetworkingClientHandler);
                RulesetNetworkingClientHandler.Dispose();
            }
            Add(RulesetNetworkingClientHandler = new RulesetNetworkingClientHandler(ClientType.Host, LocalIp.Text, Int32.Parse(HostPort.Text)));

            List<ClientInfo> list = new List<ClientInfo>();
            list.Add(RulesetNetworkingClientHandler.RulesetClientInfo);

            JoinMatch(list);
        }

        protected virtual void DirectConnect()
        {
            if (RulesetNetworkingClientHandler != null)
            {
                Remove(RulesetNetworkingClientHandler);
                RulesetNetworkingClientHandler.Dispose();
            }
            Add(RulesetNetworkingClientHandler = new RulesetNetworkingClientHandler(ClientType.Peer, HostIP.Text, Int32.Parse(HostPort.Text), LocalIp.Text));

            RulesetNetworkingClientHandler.OnConnectedToHost += (p) => JoinMatch(p);
        }

        protected virtual void JoinMatch(List<ClientInfo> clientInfos)
        {
            Remove(RulesetNetworkingClientHandler);
            MakeCurrent();
            Push(MatchScreen);
        }
    }
}
