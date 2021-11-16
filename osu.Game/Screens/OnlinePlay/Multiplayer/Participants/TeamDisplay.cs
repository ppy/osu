// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    internal class TeamDisplay : MultiplayerRoomComposite
    {
        private readonly MultiplayerRoomUser user;

        private Drawable box;

        private Sample sampleTeamSwap;

        [Resolved]
        private OsuColour colours { get; set; }

        private OsuClickableContainer clickableContent;

        public TeamDisplay(MultiplayerRoomUser user)
        {
            this.user = user;

            RelativeSizeAxes = Axes.Y;

            AutoSizeAxes = Axes.X;

            Margin = new MarginPadding { Horizontal = 3 };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            InternalChild = clickableContent = new OsuClickableContainer
            {
                Width = 15,
                Alpha = 0,
                Scale = new Vector2(0, 1),
                RelativeSizeAxes = Axes.Y,
                Child = box = new Container
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
                }
            };

            if (Client.LocalUser?.Equals(user) == true)
            {
                clickableContent.Action = changeTeam;
                clickableContent.TooltipText = "Change team";
            }

            sampleTeamSwap = audio.Samples.Get(@"Multiplayer/team-swap");
        }

        private void changeTeam()
        {
            Client.SendMatchRequest(new ChangeTeamRequest
            {
                TeamID = ((Client.LocalUser?.MatchState as TeamVersusUserState)?.TeamID + 1) % 2 ?? 0,
            });
        }

        public int? DisplayedTeam { get; private set; }

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();

            // we don't have a way of knowing when an individual user's state has updated, so just handle on RoomUpdated for now.

            var userRoomState = Room?.Users.FirstOrDefault(u => u.Equals(user))?.MatchState;

            const double duration = 400;

            int? newTeam = (userRoomState as TeamVersusUserState)?.TeamID;

            if (newTeam == DisplayedTeam)
                return;

            // only play the sample if an already valid team changes to another valid team.
            // this avoids playing a sound for each user if the match type is changed to/from a team mode.
            if (newTeam != null && DisplayedTeam != null)
                sampleTeamSwap?.Play();

            DisplayedTeam = newTeam;

            if (DisplayedTeam != null)
            {
                box.FadeColour(getColourForTeam(DisplayedTeam.Value), duration, Easing.OutQuint);
                box.ScaleTo(new Vector2(box.Scale.X < 0 ? 1 : -1, 1), duration, Easing.OutQuint);

                clickableContent.ScaleTo(Vector2.One, duration, Easing.OutQuint);
                clickableContent.FadeIn(duration);
            }
            else
            {
                clickableContent.ScaleTo(new Vector2(0, 1), duration, Easing.OutQuint);
                clickableContent.FadeOut(duration);
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
