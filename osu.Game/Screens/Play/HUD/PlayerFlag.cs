// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class PlayerFlag : CompositeDrawable, ISerialisableDrawable
    {
        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        private readonly UpdateableFlag flag;

        public PlayerFlag()
        {
            Size = new Vector2(114, 80);
            InternalChild = flag = new UpdateableFlag
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            flag.CountryCode = gameplayState.Score.ScoreInfo.User.CountryCode;
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
