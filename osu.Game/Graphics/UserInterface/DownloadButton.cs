// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class DownloadButton : GrayButton
    {
        [Resolved]
        private OsuColour colours { get; set; }

        public readonly Bindable<DownloadState> State = new Bindable<DownloadState>();

        private SpriteIcon checkmark;

        public DownloadButton()
            : base(FontAwesome.Solid.Download)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(checkmark = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                X = 8,
                Size = Vector2.Zero,
                Icon = FontAwesome.Solid.Check,
            });

            State.BindValueChanged(updateState, true);
        }

        private void updateState(ValueChangedEvent<DownloadState> state)
        {
            switch (state.NewValue)
            {
                case DownloadState.NotDownloaded:
                    Background.FadeColour(colours.Gray4, 500, Easing.InOutExpo);
                    Icon.MoveToX(0, 500, Easing.InOutExpo);
                    checkmark.ScaleTo(Vector2.Zero, 500, Easing.InOutExpo);
                    TooltipText = "Download";
                    break;

                case DownloadState.Downloading:
                    Background.FadeColour(colours.Blue, 500, Easing.InOutExpo);
                    Icon.MoveToX(0, 500, Easing.InOutExpo);
                    checkmark.ScaleTo(Vector2.Zero, 500, Easing.InOutExpo);
                    TooltipText = "Downloading...";
                    break;

                case DownloadState.Importing:
                    Background.FadeColour(colours.Yellow, 500, Easing.InOutExpo);
                    TooltipText = "Importing";
                    break;

                case DownloadState.LocallyAvailable:
                    Background.FadeColour(colours.Green, 500, Easing.InOutExpo);
                    Icon.MoveToX(-8, 500, Easing.InOutExpo);
                    checkmark.ScaleTo(new Vector2(13), 500, Easing.InOutExpo);
                    break;
            }
        }
    }
}
