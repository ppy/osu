// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Game.Screens.Play
{
    public class Spectator : OsuScreen
    {
        private readonly User targetUser;

        public Spectator([NotNull] User targetUser)
        {
            this.targetUser = targetUser ?? throw new ArgumentNullException(nameof(targetUser));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = $"Watching {targetUser}",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }
    }
}
