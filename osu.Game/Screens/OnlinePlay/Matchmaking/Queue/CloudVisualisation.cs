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
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    /// <summary>
    /// A visualisation at the top level of matchmaking which shows the overall system status.
    /// This is intended to be something which users can watch while idle, for fun or otherwise.
    /// </summary>
    public partial class CloudVisualisation : CompositeDrawable
    {
        private APIUser[] users = [];
        private Container usersContainer = null!;

        private readonly Bindable<double?> lastSamplePlayback = new Bindable<double?>();

        public APIUser[] Users
        {
            get => users;
            set
            {
                users = value;

                foreach (var u in usersContainer)
                    u.Delay(RNG.Next(0, 1000)).FadeOut(500).Expire();

                LoadComponentsAsync(users.Select(u => new MovingAvatar(u, lastSamplePlayback)), avatars =>
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

            private readonly Bindable<double?> lastSamplePlayback = new Bindable<double?>();

            private const int num_appear_samples = 6;
            private Sample? playerAppearSample;

            public MovingAvatar(APIUser apiUser, Bindable<double?> lastSamplePlayback)
                : base(apiUser)
            {
                RelativePositionAxes = Axes.Both;
                Scale = new Vector2(2);

                Origin = Anchor.Centre;
                this.lastSamplePlayback.BindTo(lastSamplePlayback);
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                playerAppearSample = audio.Samples.Get($@"Multiplayer/Matchmaking/Cloud/appear-{RNG.Next(0, num_appear_samples)}");
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
                Scheduler.AddDelayed(playAppearSample, appearDelay);
            }

            private void updateParams()
            {
                targetSpeed = RNG.NextSingle(0.05f, 0.5f);
                targetScale = RNG.NextSingle(0.2f, 3f);
                targetAlpha = RNG.NextSingle(0.5f, 1f);

                Scheduler.AddDelayed(updateParams, RNG.Next(500, 5000));
            }

            private void playAppearSample()
            {
                bool enoughTimeElapsed = !lastSamplePlayback.Value.HasValue || Time.Current - lastSamplePlayback.Value >= OsuGameBase.SAMPLE_DEBOUNCE_TIME;
                if (!enoughTimeElapsed) return;

                var chan = playerAppearSample?.GetChannel();
                if (chan == null) return;

                chan.Frequency.Value = 0.5f + RNG.NextDouble(1.5f);
                chan.Balance.Value = MathF.Cos(angle) * OsuGameBase.SFX_STEREO_STRENGTH;
                chan.Play();

                lastSamplePlayback.Value = Time.Current;
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
