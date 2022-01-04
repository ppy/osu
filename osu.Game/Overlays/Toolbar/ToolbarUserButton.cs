// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarUserButton : ToolbarOverlayToggleButton
    {
        private readonly UpdateableAvatar avatar;

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        public ToolbarUserButton()
        {
            AutoSizeAxes = Axes.X;

            DrawableText.Font = OsuFont.GetFont(italics: true);

            Add(new OpaqueBackground { Depth = 1 });

            Flow.Add(avatar = new UpdateableAvatar(isInteractive: false)
            {
                Masking = true,
                Size = new Vector2(32),
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                CornerRadius = 4,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 4,
                    Colour = Color4.Black.Opacity(0.1f),
                }
            });
        }

        [BackgroundDependencyLoader(true)]
        private void load(LoginOverlay login)
        {
            apiState.BindTo(api.State);
            apiState.BindValueChanged(onlineStateChanged, true);

            StateContainer = login;
        }

        private void onlineStateChanged(ValueChangedEvent<APIState> state) => Schedule(() =>
        {
            switch (state.NewValue)
            {
                default:
                    Text = @"Guest";
                    avatar.User = new APIUser();
                    break;

                case APIState.Online:
                    Text = api.LocalUser.Value.Username;
                    avatar.User = api.LocalUser.Value;
                    break;
            }
        });
    }
}
