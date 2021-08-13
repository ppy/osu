// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    internal class TeamDisplay : MultiplayerRoomComposite
    {
        private readonly User user;
        private Drawable box;

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private MultiplayerClient client { get; set; }

        public TeamDisplay(User user)
        {
            this.user = user;

            RelativeSizeAxes = Axes.Y;
            Width = 15;

            Margin = new MarginPadding { Horizontal = 3 };

            Alpha = 0;
            Scale = new Vector2(0, 1);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            box = new Container
            {
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 5,
                Masking = true,
                Scale = new Vector2(0, 1),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = new Box
                {
                    Colour = Color4.White,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };

            if (user.Id == client.LocalUser?.UserID)
            {
                InternalChild = new OsuClickableContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    TooltipText = "Change team",
                    Action = changeTeam,
                    Child = box
                };
            }
            else
            {
                InternalChild = box;
            }
        }

        private void changeTeam()
        {
            client.SendMatchRequest(new ChangeTeamRequest
            {
                TeamID = ((client.LocalUser?.MatchState as TeamVersusUserState)?.TeamID + 1) % 2 ?? 0,
            });
        }

        private int? displayedTeam;

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();

            // we don't have a way of knowing when an individual user's state has updated, so just handle on RoomUpdated for now.

            var userRoomState = Room?.Users.FirstOrDefault(u => u.UserID == user.Id)?.MatchState;

            const double duration = 400;

            int? newTeam = (userRoomState as TeamVersusUserState)?.TeamID;

            if (newTeam == displayedTeam)
                return;

            displayedTeam = newTeam;

            if (displayedTeam != null)
            {
                box.FadeColour(getColourForTeam(displayedTeam.Value), duration, Easing.OutQuint);
                box.ScaleTo(new Vector2(box.Scale.X < 0 ? 1 : -1, 1), duration, Easing.OutQuint);

                this.ScaleTo(Vector2.One, duration, Easing.OutQuint);
                this.FadeIn(duration);
            }
            else
            {
                this.ScaleTo(new Vector2(0, 1), duration, Easing.OutQuint);
                this.FadeOut(duration);
            }
        }

        private ColourInfo getColourForTeam(int id)
        {
            switch (id)
            {
                default:
                    return colours.Red;

                case 1:
                    return colours.Blue;
            }
        }
    }
}
