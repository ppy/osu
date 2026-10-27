// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class AddToPlaylistFooterButton : ShearedButtonWithIcon
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Width = 220;

            DarkerColour = colours.Blue3;
            LighterColour = colours.Blue1;

            Text = OnlinePlayStrings.FooterButtonPlaylistAdd;
            Icon = OsuIcon.Add;
        }

        public void Appear()
        {
            FinishTransforms();

            this.MoveToY(150f)
                .FadeOut()
                .MoveToY(0f, 240, Easing.OutCubic)
                .FadeIn(240, Easing.OutCubic);
        }

        public TransformSequence<AddToPlaylistFooterButton> Disappear()
        {
            FinishTransforms();

            return this.FadeOut(240, Easing.InOutCubic)
                       .MoveToY(150f, 240, Easing.InOutCubic);
        }
    }
}
