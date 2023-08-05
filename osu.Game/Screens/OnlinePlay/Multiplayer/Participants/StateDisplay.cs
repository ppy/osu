// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public partial class StateDisplay : CompositeDrawable
    {
        private const double fade_time = 50;

        private SpriteIcon icon = null!;
        private OsuSpriteText text = null!;
        private ProgressBar progressBar = null!;

        public StateDisplay()
        {
            AutoSizeAxes = Axes.Both;
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            this.colours = colours;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Icon = FontAwesome.Solid.CheckCircle,
                        Size = new Vector2(12),
                    },
                    new CircularContainer
                    {
                        Masking = true,
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Children = new Drawable[]
                        {
                            progressBar = new ProgressBar(false)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                BackgroundColour = Color4.Black.Opacity(0.4f),
                                FillColour = colours.Blue,
                                Alpha = 0f,
                            },
                            text = new OsuSpriteText
                            {
                                Padding = new MarginPadding { Horizontal = 5f, Vertical = 1f },
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Font = OsuFont.GetFont(weight: FontWeight.Regular, size: 12),
                                Colour = Color4Extensions.FromHex("#DDFFFF")
                            },
                        }
                    },
                }
            };
        }

        private OsuColour colours = null!;

        public void UpdateStatus(MultiplayerUserState state, BeatmapAvailability availability)
        {
            // the only case where the progress bar is used does its own local fade in.
            // starting by fading out is a sane default.
            progressBar.FadeOut(fade_time);
            this.FadeIn(fade_time);

            switch (state)
            {
                case MultiplayerUserState.Idle:
                    showBeatmapAvailability(availability);
                    break;

                case MultiplayerUserState.Ready:
                    text.Text = "ready";
                    icon.Icon = FontAwesome.Solid.CheckCircle;
                    icon.Colour = Color4Extensions.FromHex("#AADD00");
                    break;

                case MultiplayerUserState.WaitingForLoad:
                    text.Text = "loading";
                    icon.Icon = FontAwesome.Solid.PauseCircle;
                    icon.Colour = colours.Yellow;
                    break;

                case MultiplayerUserState.Loaded:
                case MultiplayerUserState.ReadyForGameplay:
                    text.Text = "loaded";
                    icon.Icon = FontAwesome.Solid.DotCircle;
                    icon.Colour = colours.YellowLight;
                    break;

                case MultiplayerUserState.Playing:
                    text.Text = "playing";
                    icon.Icon = FontAwesome.Solid.PlayCircle;
                    icon.Colour = colours.BlueLight;
                    break;

                case MultiplayerUserState.FinishedPlay:
                    text.Text = "results pending";
                    icon.Icon = FontAwesome.Solid.ArrowAltCircleUp;
                    icon.Colour = colours.BlueLighter;
                    break;

                case MultiplayerUserState.Results:
                    text.Text = "results";
                    icon.Icon = FontAwesome.Solid.ArrowAltCircleUp;
                    icon.Colour = colours.BlueLighter;
                    break;

                case MultiplayerUserState.Spectating:
                    text.Text = "spectating";
                    icon.Icon = FontAwesome.Solid.Binoculars;
                    icon.Colour = colours.BlueLight;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void showBeatmapAvailability(BeatmapAvailability availability)
        {
            switch (availability.State)
            {
                default:
                    this.FadeOut(fade_time);
                    break;

                case DownloadState.Unknown:
                    text.Text = "checking availability";
                    icon.Icon = FontAwesome.Solid.Question;
                    icon.Colour = colours.Orange0;
                    break;

                case DownloadState.NotDownloaded:
                    text.Text = "no map";
                    icon.Icon = FontAwesome.Solid.MinusCircle;
                    icon.Colour = colours.RedLight;
                    break;

                case DownloadState.Downloading:
                    progressBar.FadeIn(fade_time);
                    progressBar.CurrentTime = availability.DownloadProgress ?? 0;

                    text.Text = "downloading map";
                    icon.Icon = FontAwesome.Solid.ArrowAltCircleDown;
                    icon.Colour = colours.Blue;
                    break;

                case DownloadState.Importing:
                    text.Text = "importing map";
                    icon.Icon = FontAwesome.Solid.ArrowAltCircleDown;
                    icon.Colour = colours.Yellow;
                    break;
            }
        }
    }
}
