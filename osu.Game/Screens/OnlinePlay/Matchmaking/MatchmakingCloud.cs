// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public partial class MatchmakingCloud : CompositeDrawable
    {
        private APIUser[] users = [];
        private Container usersContainer = null!;

        public APIUser[] Users
        {
            get => users;
            set
            {
                users = value;

                foreach (var u in usersContainer)
                    u.Delay(RNG.Next(0, 1000)).FadeOut(500).Expire();

                LoadComponentsAsync(users.Select(u => new MovingAvatar(u)), avatars =>
                {
                    if (usersContainer.Count == 0)
                    {
                        usersContainer.ScaleTo(0)
                                      .ScaleTo(1, 5000, Easing.OutPow10);
                    }

                    usersContainer.AddRange(avatars);
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChildren = new Drawable[]
            {
                usersContainer = new AspectContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                },
            };
        }

        public partial class MovingAvatar : MatchmakingAvatar
        {
            private float angle;
            private float angularSpeed;

            private float targetSpeed;
            private float targetScale;
            private float targetAlpha;

            private Bindable<double?> lastSamplePlaybackTime = null!;

            private Sample? playerAppearSample;

            public MovingAvatar(APIUser apiUser)
                : base(apiUser)
            {
                RelativePositionAxes = Axes.Both;
                Scale = new Vector2(2);

                Origin = Anchor.Centre;
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio, SessionStatics statics)
            {
                playerAppearSample = audio.Samples.Get(@"UI/toolbar-hover");
                lastSamplePlaybackTime = statics.GetBindable<double?>(Static.LastMatchmakingCloudSamplePlaybackTime);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateParams();

                angle = RNG.NextSingle(0f, MathF.Tau);

                angularSpeed = targetSpeed;
                Scale = new Vector2(targetScale);

                Hide();
                int appearDelay = RNG.Next(0, 1000);
                this.Delay(appearDelay).FadeTo(targetAlpha, 2000, Easing.OutQuint);
                Scheduler.AddDelayed(() =>
                {
                    bool enoughTimeElapsed = !lastSamplePlaybackTime.Value.HasValue || Time.Current - lastSamplePlaybackTime.Value >= OsuGameBase.SAMPLE_DEBOUNCE_TIME;
                    if (!enoughTimeElapsed) return;

                    var chan = playerAppearSample?.GetChannel();

                    if (chan == null) return;

                    chan.Frequency.Value = 1f + RNG.NextDouble(0.25f);
                    chan.Play();

                    lastSamplePlaybackTime.Value = Time.Current;
                }, appearDelay);
            }

            private void updateParams()
            {
                targetSpeed = RNG.NextSingle(0.05f, 0.5f);
                targetScale = RNG.NextSingle(0.2f, 3f);
                targetAlpha = RNG.NextSingle(0.5f, 1f);

                Scheduler.AddDelayed(updateParams, RNG.Next(500, 5000));
            }

            protected override void Update()
            {
                base.Update();

                float elapsed = (float)Math.Min(20, Time.Elapsed) / 1000;

                Scale = new Vector2((float)Interpolation.Lerp(Scale.X, targetScale, elapsed / 100));
                Alpha = (float)Interpolation.Lerp(Alpha, targetAlpha, elapsed / 100);
                angularSpeed = (float)Interpolation.Lerp(angularSpeed, targetSpeed, elapsed / 100);

                angle += angularSpeed * elapsed * 0.5f;

                Position = new Vector2(0.5f) +
                           new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * angularSpeed;
            }
        }
    }
}
