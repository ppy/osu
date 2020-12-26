// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public class StateDisplay : CompositeDrawable
    {
        public StateDisplay()
        {
            AutoSizeAxes = Axes.Both;
            Alpha = 0;
        }

        private MultiplayerUserState status;

        private OsuSpriteText text;
        private SpriteIcon icon;

        private const double fade_time = 50;

        public MultiplayerUserState Status
        {
            set
            {
                if (value == status)
                    return;

                status = value;

                if (IsLoaded)
                    updateStatus();
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(weight: FontWeight.Regular, size: 12),
                        Colour = Color4Extensions.FromHex("#DDFFFF")
                    },
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = FontAwesome.Solid.CheckCircle,
                        Size = new Vector2(12),
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateStatus();
        }

        [Resolved]
        private OsuColour colours { get; set; }

        private void updateStatus()
        {
            switch (status)
            {
                default:
                    this.FadeOut(fade_time);
                    return;

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
            }

            this.FadeIn(fade_time);
        }
    }
}
