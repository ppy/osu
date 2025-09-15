// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick
{
    public partial class BeatmapSelectionOverlay : CompositeDrawable
    {
        private readonly Dictionary<int, SelectionAvatar> avatars = new Dictionary<int, SelectionAvatar>();

        private readonly Container<SelectionAvatar> avatarContainer;

        private Sample? userAddedSample;
        private double? lastSamplePlayback;

        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            set => base.AutoSizeAxes = value;
        }

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        public BeatmapSelectionOverlay()
        {
            InternalChild = avatarContainer = new Container<SelectionAvatar>();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            avatarContainer.AutoSizeAxes = AutoSizeAxes;
            avatarContainer.RelativeSizeAxes = RelativeSizeAxes;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            userAddedSample = audio.Samples.Get(@"Multiplayer/player-ready");
        }

        public bool AddUser(APIUser user, bool isOwnUser)
        {
            if (avatars.ContainsKey(user.Id))
                return false;

            var avatar = new SelectionAvatar(user, isOwnUser)
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
            };

            avatarContainer.Add(avatars[user.Id] = avatar);

            if (lastSamplePlayback == null || Time.Current - lastSamplePlayback > OsuGameBase.SAMPLE_DEBOUNCE_TIME)
            {
                userAddedSample?.Play();
                lastSamplePlayback = Time.Current;
            }

            updateLayout();

            avatar.FinishTransforms();

            return true;
        }

        public bool RemoveUser(int id)
        {
            if (!avatars.Remove(id, out var avatar))
                return false;

            avatar.PopOutAndExpire();
            avatarContainer.ChangeChildDepth(avatar, float.MaxValue);

            updateLayout();

            return true;
        }

        private void updateLayout()
        {
            const double stagger = 30;
            const float spacing = 4;

            double delay = 0;
            float x = 0;

            for (int i = avatarContainer.Count - 1; i >= 0; i--)
            {
                var avatar = avatarContainer[i];

                if (avatar.Expired)
                    continue;

                avatar.Delay(delay).MoveToX(x, 500, Easing.OutElasticQuarter);

                x -= avatar.LayoutSize.X + spacing;

                delay += stagger;
            }
        }

        public partial class SelectionAvatar : CompositeDrawable
        {
            public bool Expired { get; private set; }

            private readonly Container content;

            public SelectionAvatar(APIUser user, bool isOwnUser)
            {
                Size = new Vector2(30);

                InternalChildren = new Drawable[]
                {
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Child = new MatchmakingAvatar(user, isOwnUser)
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                content.ScaleTo(0)
                       .ScaleTo(1, 500, Easing.OutElasticHalf)
                       .FadeIn(200);
            }

            public void PopOutAndExpire()
            {
                content.ScaleTo(0, 400, Easing.OutExpo);

                this.FadeOut(100).Expire();
                Expired = true;
            }
        }
    }
}
