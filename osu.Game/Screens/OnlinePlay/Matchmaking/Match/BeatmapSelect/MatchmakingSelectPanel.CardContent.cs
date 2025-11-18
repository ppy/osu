// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class MatchmakingSelectPanel
    {
        public abstract partial class CardContent : CompositeDrawable
        {
            public abstract AvatarOverlay SelectionOverlay { get; }

            protected CardContent()
            {
                RelativeSizeAxes = Axes.Both;
            }

            public partial class AvatarOverlay : CompositeDrawable
            {
                private readonly Container<SelectionAvatar> avatars;

                private Sample? userAddedSample;
                private double? lastSamplePlayback;

                [Resolved]
                private IAPIProvider api { get; set; } = null!;

                public AvatarOverlay()
                {
                    AutoSizeAxes = Axes.Both;

                    InternalChild = avatars = new Container<SelectionAvatar>
                    {
                        AutoSizeAxes = Axes.X,
                        Height = SelectionAvatar.AVATAR_SIZE,
                    };

                    Padding = new MarginPadding { Vertical = 5 };
                }

                [BackgroundDependencyLoader]
                private void load(AudioManager audio)
                {
                    userAddedSample = audio.Samples.Get(@"Multiplayer/player-ready");
                }

                public bool AddUser(APIUser user)
                {
                    if (avatars.Any(a => a.User.Id == user.Id))
                        return false;

                    var avatar = new SelectionAvatar(user, user.Equals(api.LocalUser.Value));

                    avatars.Add(avatar);

                    if (lastSamplePlayback == null || Time.Current - lastSamplePlayback > OsuGameBase.SAMPLE_DEBOUNCE_TIME)
                    {
                        userAddedSample?.Play();
                        lastSamplePlayback = Time.Current;
                    }

                    updateAvatarLayout();

                    avatar.FinishTransforms();

                    return true;
                }

                public bool RemoveUser(int id)
                {
                    if (avatars.SingleOrDefault(a => a.User.Id == id) is not SelectionAvatar avatar)
                        return false;

                    avatar.PopOutAndExpire();
                    avatars.ChangeChildDepth(avatar, float.MaxValue);

                    updateAvatarLayout();

                    return true;
                }

                private void updateAvatarLayout()
                {
                    const double stagger = 30;
                    const float spacing = 4;

                    double delay = 0;
                    float x = 0;

                    for (int i = avatars.Count - 1; i >= 0; i--)
                    {
                        var avatar = avatars[i];

                        if (avatar.Expired)
                            continue;

                        avatar.Delay(delay).MoveToX(x, 500, Easing.OutElasticQuarter);

                        x -= avatar.LayoutSize.X + spacing;

                        delay += stagger;
                    }
                }

                public partial class SelectionAvatar : CompositeDrawable
                {
                    public const float AVATAR_SIZE = 30;

                    public APIUser User { get; }

                    public bool Expired { get; private set; }

                    private readonly MatchmakingAvatar avatar;

                    public SelectionAvatar(APIUser user, bool isOwnUser)
                    {
                        User = user;
                        Size = new Vector2(AVATAR_SIZE);

                        InternalChild = avatar = new MatchmakingAvatar(user, isOwnUser)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        };
                    }

                    protected override void LoadComplete()
                    {
                        base.LoadComplete();

                        avatar.ScaleTo(0)
                              .ScaleTo(1, 500, Easing.OutElasticHalf)
                              .FadeIn(200);
                    }

                    public void PopOutAndExpire()
                    {
                        avatar.ScaleTo(0, 400, Easing.OutExpo);

                        this.FadeOut(100).Expire();
                        Expired = true;
                    }
                }
            }
        }
    }
}
