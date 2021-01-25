// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public class TournamentModDisplay : CompositeDrawable
    {
        public string ModAcronym;

        [Resolved]
        private LadderInfo ladderInfo { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            var texture = textures.Get($"mods/{ModAcronym}");

            if (texture != null)
            {
                AddInternal(new Sprite
                {
                    FillMode = FillMode.Fit,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Texture = texture
                });
            }
            else
            {
                var mod = rulesets.GetRuleset(ladderInfo.Ruleset.Value.ID ?? 0).CreateInstance().GetAllMods().FirstOrDefault(mod => mod.Acronym == ModAcronym);

                AddInternal(new ModIcon(mod)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.5f)
                });
            }
        }
    }
}
