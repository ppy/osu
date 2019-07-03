// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public class DownloadButton : OsuAnimatedButton
    {
        public readonly Bindable<DownloadState> State = new Bindable<DownloadState>();

        private readonly SpriteIcon icon;
        private readonly SpriteIcon checkmark;
        private readonly Box background;

        private OsuColour colours;

        public DownloadButton()
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(13),
                    Icon = FontAwesome.Solid.Download,
                },
                checkmark = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    X = 8,
                    Size = Vector2.Zero,
                    Icon = FontAwesome.Solid.Check,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            this.colours = colours;

            State.BindValueChanged(updateState, true);
        }

        private void updateState(ValueChangedEvent<DownloadState> state)
        {
            switch (state.NewValue)
            {
                case DownloadState.NotDownloaded:
                    background.FadeColour(colours.Gray4, 500, Easing.InOutExpo);
                    icon.MoveToX(0, 500, Easing.InOutExpo);
                    checkmark.ScaleTo(Vector2.Zero, 500, Easing.InOutExpo);
                    break;

                case DownloadState.Downloading:
                    background.FadeColour(colours.Blue, 500, Easing.InOutExpo);
                    icon.MoveToX(0, 500, Easing.InOutExpo);
                    checkmark.ScaleTo(Vector2.Zero, 500, Easing.InOutExpo);
                    break;

                case DownloadState.Downloaded:
                    background.FadeColour(colours.Yellow, 500, Easing.InOutExpo);
                    break;

                case DownloadState.LocallyAvailable:
                    background.FadeColour(colours.Green, 500, Easing.InOutExpo);
                    icon.MoveToX(-8, 500, Easing.InOutExpo);
                    checkmark.ScaleTo(new Vector2(13), 500, Easing.InOutExpo);
                    break;
            }
        }
    }
}
