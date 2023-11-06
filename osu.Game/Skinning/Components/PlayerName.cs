// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play;

namespace osu.Game.Skinning.Components
{
    [UsedImplicitly]
    public partial class PlayerName : FontAdjustableSkinComponent
    {
        private readonly OsuSpriteText text;

        public PlayerName()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(GameplayState gameplayState)
        {
            text.Text = gameplayState.Score.ScoreInfo.User.Username;
        }

        protected override void SetFont(FontUsage font) => text.Font = font.With(size: 40);
    }
}
