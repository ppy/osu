// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Skinning;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class SkinnableAvatar : CompositeDrawable, ISerialisableDrawable
    {
        [SettingSource("Corner radius", "How much the edges should be rounded.")]
        public new BindableFloat CornerRadius { get; set; } = new BindableFloat
        {
            MinValue = 0,
            MaxValue = 63,
            Precision = 0.01f
        };

        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        private readonly UpdateableAvatar avatar;

        public SkinnableAvatar()
        {
            Size = new Vector2(128f);
            InternalChild = avatar = new UpdateableAvatar(isInteractive: false)
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            avatar.User = gameplayState.Score.ScoreInfo.User;
            CornerRadius.BindValueChanged(e => avatar.CornerRadius = e.NewValue);
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
