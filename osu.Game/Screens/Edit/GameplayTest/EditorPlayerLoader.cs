// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Edit.GameplayTest
{
    public class EditorPlayerLoader : PlayerLoader
    {
        [Resolved]
        private OsuLogo osuLogo { get; set; }

        public EditorPlayerLoader(Editor editor)
            : base(() => new EditorPlayer(editor))
        {
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            MetadataInfo.FinishTransforms(true);
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            // call base with resuming forcefully set to true to reduce logo movements.
            base.LogoArriving(logo, true);
            logo.FinishTransforms(true, nameof(Scale));
        }

        protected override void ContentOut()
        {
            base.ContentOut();
            osuLogo.FadeOut(CONTENT_OUT_DURATION, Easing.OutQuint);
        }

        protected override double PlayerPushDelay => 0;
    }
}
