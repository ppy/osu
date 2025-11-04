// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerSkipOverlay : SkipOverlay
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private Drawable votedIcon = null!;
        private OsuSpriteText countText = null!;

        public MultiplayerSkipOverlay(double startTime)
            : base(startTime)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            FadingContent.AddRange(
            [
                votedIcon = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Position = new Vector2(50, 0),
                    Size = new Vector2(20),
                    Alpha = 0,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Green
                        },
                        new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Scale = new Vector2(0.5f),
                            Icon = FontAwesome.Solid.Check
                        }
                    }
                },
                countText = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.X,
                    Position = new Vector2(0.75f, 0),
                    Font = OsuFont.Default.With(size: 36, weight: FontWeight.Bold)
                }
            ]);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.UserLeft += onUserLeft;
            client.UserStateChanged += onUserStateChanged;
            client.UserVotedToSkipIntro += onUserVotedToSkipIntro;

            updateText();
        }

        private void onUserLeft(MultiplayerRoomUser user)
        {
            Schedule(updateText);
        }

        private void onUserStateChanged(MultiplayerRoomUser user, MultiplayerUserState state)
        {
            Schedule(updateText);
        }

        private void onUserVotedToSkipIntro(int userId) => Schedule(() =>
        {
            updateText();

            countText.ScaleTo(1.5f).ScaleTo(1, 200, Easing.OutSine);

            if (userId == client.LocalUser?.UserID)
            {
                votedIcon.ScaleTo(1.5f).ScaleTo(1, 200, Easing.OutSine);
                votedIcon.FadeInFromZero(100);
            }
        });

        private void updateText()
        {
            if (client.Room == null || client.Room.Settings.AutoSkip)
                return;

            int countTotal = client.Room.Users.Count(u => u.State == MultiplayerUserState.Playing);
            int countSkipped = client.Room.Users.Count(u => u.State == MultiplayerUserState.Playing && u.VotedToSkipIntro);
            int countRequired = countTotal / 2 + 1;

            countText.Text = $"{Math.Min(countRequired, countSkipped)} / {countRequired}";
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.UserLeft -= onUserLeft;
                client.UserStateChanged -= onUserStateChanged;
                client.UserVotedToSkipIntro -= onUserVotedToSkipIntro;
            }
        }
    }
}
