// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
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

        [Resolved]
        private GameplayState? gameplayState { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private IBindable<APIUser>? apiUser;

        private readonly Container cornerContainer;

        public PlayerAvatar()
        {
            Size = new Vector2(default_size);

            InternalChild = cornerContainer = new Container
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Child = avatar = new UpdateableAvatar(isInteractive: false)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (gameplayState != null)
                avatar.User = gameplayState.Score.ScoreInfo.User;
            else
            {
                apiUser = api.LocalUser.GetBoundCopy();
                apiUser.BindValueChanged(u => avatar.User = u.NewValue, true);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CornerRadius.BindValueChanged(e => cornerContainer.CornerRadius = e.NewValue * default_size, true);
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
