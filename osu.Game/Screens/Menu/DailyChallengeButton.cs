// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.DailyChallenge;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Menu
{
    public partial class DailyChallengeButton : MainMenuButton
    {
        public Room? Room { get; private set; }

        private readonly OsuSpriteText countdown;
        private ScheduledDelegate? scheduledCountdownUpdate;

        private UpdateableOnlineBeatmapSetCover cover = null!;
        private IBindable<DailyChallengeInfo?> info = null!;

        private Box gradientLayer = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        public DailyChallengeButton(string sampleName, Color4 colour, Action<MainMenuButton>? clickAction = null, params Key[] triggerKeys)
            : base(ButtonSystemStrings.DailyChallenge, sampleName, OsuIcon.DailyChallenge, colour, clickAction, triggerKeys)
        {
            BaseSize = new Vector2(ButtonSystem.BUTTON_WIDTH * 1.3f, ButtonArea.BUTTON_AREA_HEIGHT);

            Content.Add(countdown = new OsuSpriteText
            {
                Shadow = true,
                AllowMultiline = false,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Margin = new MarginPadding
                {
                    Left = -3,
                    Bottom = 22,
                },
                Font = OsuFont.Default.With(size: 12),
                Alpha = 0,
            });
        }

        protected override Drawable CreateBackground(Colour4 accentColour) => new BufferedContainer
        {
            Children = new Drawable[]
            {
                cover = new UpdateableOnlineBeatmapSetCover(timeBeforeLoad: 0, timeBeforeUnload: 600_000)
                {
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                },
                gradientLayer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(accentColour.Opacity(0.2f), accentColour),
                    Blending = BlendingParameters.Additive,
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = accentColour.Opacity(0.7f)
                },
            },
        };

        [BackgroundDependencyLoader]
        private void load(MetadataClient metadataClient)
        {
            info = metadataClient.DailyChallengeInfo.GetBoundCopy();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            info.BindValueChanged(dailyChallengeChanged, true);
        }

        protected override void Update()
        {
            base.Update();

            if (cover.LatestTransformEndTime == Time.Current)
            {
                const double duration = 3000;

                float scale = 1 + RNG.NextSingle();

                cover.ScaleTo(scale, duration, Easing.InOutSine)
                     .RotateTo(RNG.NextSingle(-4, 4) * (scale - 1), duration, Easing.InOutSine)
                     .MoveTo(new Vector2(
                         RNG.NextSingle(-0.5f, 0.5f) * (scale - 1),
                         RNG.NextSingle(-0.5f, 0.5f) * (scale - 1)
                     ), duration, Easing.InOutSine);

                gradientLayer.FadeIn(duration / 2)
                             .Then()
                             .FadeOut(duration / 2);
            }
        }

        private long? lastNotifiedDailyChallengeRoomId;

        private void dailyChallengeChanged(ValueChangedEvent<DailyChallengeInfo?> _)
        {
            UpdateState();

            scheduledCountdownUpdate?.Cancel();
            scheduledCountdownUpdate = null;

            if (info.Value == null)
            {
                Room = null;
                cover.OnlineInfo = TooltipContent = null;
            }
            else
            {
                var roomRequest = new GetRoomRequest(info.Value.Value.RoomID);

                roomRequest.Success += room =>
                {
                    Room = room;
                    cover.OnlineInfo = TooltipContent = room.Playlist.FirstOrDefault()?.Beatmap.BeatmapSet as APIBeatmapSet;

                    // We only want to notify the user if a new challenge recently went live.
                    if (room.StartDate.Value != null
                        && Math.Abs((DateTimeOffset.Now - room.StartDate.Value!.Value).TotalSeconds) < 1800
                        && room.RoomID.Value != lastNotifiedDailyChallengeRoomId)
                    {
                        lastNotifiedDailyChallengeRoomId = room.RoomID.Value;
                        notificationOverlay?.Post(new NewDailyChallengeNotification(room));
                    }

                    updateCountdown();
                    Scheduler.AddDelayed(updateCountdown, 1000, true);
                };
                api.Queue(roomRequest);
            }
        }

        private void updateCountdown()
        {
            if (Room == null)
                return;

            var remaining = (Room.EndDate.Value - DateTimeOffset.Now) ?? TimeSpan.Zero;

            if (remaining <= TimeSpan.Zero)
            {
                countdown.FadeOut(250, Easing.OutQuint);
            }
            else
            {
                if (countdown.Alpha == 0)
                    countdown.FadeIn(250, Easing.OutQuint);

                countdown.Text = remaining.ToString(@"hh\:mm\:ss");
            }
        }

        protected override void UpdateState()
        {
            if (info.IsNotNull() && info.Value == null)
            {
                ContractStyle = 0;
                State = ButtonState.Contracted;
                return;
            }

            base.UpdateState();
        }

        public APIBeatmapSet? TooltipContent { get; private set; }
    }
}
