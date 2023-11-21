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
        private readonly BaseUpdateableFlag flag;

        private const float default_size = 40f;

        public PlayerFlag()
        {
            Size = new Vector2(default_size, default_size / 1.4f);
            InternalChild = flag = new BaseUpdateableFlag
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        [BackgroundDependencyLoader]
        private void load(GameplayState gameplayState)
        {
            flag.CountryCode = gameplayState.Score.ScoreInfo.User.CountryCode;
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
