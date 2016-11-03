using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;

namespace osu.Game.Overlays.Options
{
    public class LoginOptions : OptionsSubsection
    {
        public LoginOptions(APIAccess api)
        {
            var state = api == null ? APIAccess.APIState.Offline : api.State;
            Header = "Sign In";
            Children = new[]
            {
                new Container
                {
                    Alpha = state == APIAccess.APIState.Online ? 1 : 0,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new[]
                    {
                        new SpriteText { Text = $"Logged in as {api?.Username}" }
                    }
                },
                new Container
                {
                    Alpha = state == APIAccess.APIState.Offline ? 1 : 0,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new[] { new LoginForm() }
                },
            };
        }
    }

    public class LoginForm : FlowContainer
    {
        public LoginForm()
        {
            Direction = FlowDirection.VerticalOnly;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Spacing = new Vector2(0, 5);
            // TODO: Wire things up
            Children = new Drawable[]
            {
                new SpriteText { Text = "Username" },
                new TextBox { Height = 20, RelativeSizeAxes = Axes.X },
                new SpriteText { Text = "Password" },
                new TextBox { Height = 20, RelativeSizeAxes = Axes.X },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Log in",
                }
            };
        }
    }
}