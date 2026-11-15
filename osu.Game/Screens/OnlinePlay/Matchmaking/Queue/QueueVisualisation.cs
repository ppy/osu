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
    public partial class QueueVisualisation : CompositeDrawable
    {
        private const int min_scale = 1;
        private const int max_scale = 3;

        private APIUser[] users = [];
        private readonly Container usersContainer;

        private readonly Bindable<double?> lastSamplePlayback = new Bindable<double?>();

        /// <summary>
        /// Relative radius from the centre which avatars should not be displayed inside.
        /// </summary>
        public float SafeRadius { get; init; } = 0.25f;

        public APIUser[] Users
        {
            get => users;
            set
            {
                users = value;
                if (IsLoaded)
                    refresh();
            }
        }

        private void refresh()
        {
            foreach (var u in usersContainer)
                u.Delay(RNG.Next(0, 1000)).FadeOut(500).Expire();

            LoadComponentsAsync(users.Select(u => new MovingAvatar(u, SafeRadius)
            {
                LastSamplePlayback = { BindTarget = lastSamplePlayback },
            }), avatars =>
            {
                if (usersContainer.Count == 0)
                {
                    usersContainer.ScaleTo(0)
                                  .ScaleTo(1, 5000, Easing.OutPow10);
                }

                usersContainer.AddRange(avatars);
            });
        }

        public QueueVisualisation()
        {
            InternalChildren = new Drawable[]
            {
                usersContainer = new AspectContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            refresh();
        }

        public partial class MovingAvatar : MatchmakingAvatar
        {
            private float angle;
            private float radius;

            private float targetScale;
            private float targetAlpha;

            private readonly float safeRadius;

            public readonly Bindable<double?> LastSamplePlayback = new Bindable<double?>();

            private const int num_appear_samples = 6;
            private Sample? playerAppearSample;

            public MovingAvatar(APIUser apiUser, float safeRadius)
                : base(apiUser)
            {
                RelativePositionAxes = Axes.Both;
                Scale = new Vector2(2);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                this.safeRadius = safeRadius;
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
                radius = RNG.NextSingle(safeRadius, 0.5f);

                Scale = new Vector2(targetScale);

                Hide();
                int appearDelay = RNG.Next(0, 1000);
                this.Delay(appearDelay).FadeTo(targetAlpha, 2000, Easing.OutQuint);
                Scheduler.AddDelayed(playAppearSample, appearDelay);
            }

            private void updateParams()
            {
                targetScale = RNG.NextSingle(min_scale, max_scale);
                targetAlpha = RNG.NextSingle(0.5f, 1f);

                Scheduler.AddDelayed(updateParams, RNG.Next(500, 5000));
            }

            private void playAppearSample()
            {
                bool enoughTimeElapsed = !LastSamplePlayback.Value.HasValue || Time.Current - LastSamplePlayback.Value >= OsuGameBase.SAMPLE_DEBOUNCE_TIME;
                if (!enoughTimeElapsed) return;

                var chan = playerAppearSample?.GetChannel();
                if (chan == null) return;

                chan.Frequency.Value = 0.5f + RNG.NextDouble(1.5f);
                chan.Balance.Value = MathF.Cos(angle) * OsuGameBase.SFX_STEREO_STRENGTH;
                chan.Play();

                LastSamplePlayback.Value = Time.Current;
            }

            protected override void Update()
            {
                base.Update();

                float elapsed = (float)Math.Min(20, Time.Elapsed) / 1000;

                Scale = new Vector2((float)Interpolation.Lerp(Scale.X, targetScale, elapsed / 100));
                Alpha = (float)Interpolation.Lerp(Alpha, targetAlpha, elapsed / 100);

                angle += radius * elapsed * 0.5f;

                Position = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            }
        }
    }
}
