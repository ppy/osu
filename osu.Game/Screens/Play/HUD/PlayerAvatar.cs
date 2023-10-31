// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Overlays.Settings;
using osu.Game.Skinning;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class PlayerAvatar : CompositeDrawable, ISerialisableDrawable
    {
        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.CornerRadius), nameof(SkinnableComponentStrings.CornerRadiusDescription),
            SettingControlType = typeof(SettingsPercentageSlider<float>))]
        public new BindableFloat CornerRadius { get; } = new BindableFloat(0.25f)
        {
            MinValue = 0,
            MaxValue = 0.5f,
            Precision = 0.01f
        };

        private readonly UpdateableAvatar avatar;

        private const float default_size = 80f;

        public PlayerAvatar()
        {
            Size = new Vector2(default_size);

            InternalChild = avatar = new UpdateableAvatar(isInteractive: false)
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true
            };
        }

        [BackgroundDependencyLoader]
        private void load(GameplayState gameplayState)
        {
            avatar.User = gameplayState.Score.ScoreInfo.User;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CornerRadius.BindValueChanged(e => avatar.CornerRadius = e.NewValue * default_size, true);
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
