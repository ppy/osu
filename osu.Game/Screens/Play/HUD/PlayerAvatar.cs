// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Skinning;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class PlayerAvatar : CompositeDrawable, ISerialisableDrawable
    {
        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.CornerRadius), nameof(SkinnableComponentStrings.CornerRadiusDescription))]
        public new BindableFloat CornerRadius { get; set; } = new BindableFloat
        {
            MinValue = 0,
            MaxValue = 63,
            Precision = 0.01f
        };

        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        private readonly UpdateableAvatar avatar;

        public PlayerAvatar()
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
            avatar.CornerRadius = CornerRadius.Value;
            CornerRadius.BindValueChanged(e => avatar.CornerRadius = e.NewValue);
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
