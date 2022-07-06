// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModMirror : ModMirror, IApplicableToBeatmap
    {
        public override string Description => "Fruits are flipped horizontally.";

        /// <remarks>
        /// <see cref="IApplicableToBeatmap"/> is used instead of <see cref="IApplicableToHitObject"/>,
        /// as <see cref="CatchBeatmapProcessor"/> applies offsets in <see cref="CatchBeatmapProcessor.PostProcess"/>.
        /// <see cref="IApplicableToBeatmap"/> runs after post-processing, while <see cref="IApplicableToHitObject"/> runs before it.
        /// </remarks>
        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            foreach (var hitObject in beatmap.HitObjects)
                applyToHitObject(hitObject);
        }

        private void applyToHitObject(HitObject hitObject)
        {
            var catchObject = (CatchHitObject)hitObject;

            switch (catchObject)
            {
                case Fruit fruit:
                    mirrorEffectiveX(fruit);
                    break;

                case JuiceStream juiceStream:
                    mirrorEffectiveX(juiceStream);
                    mirrorJuiceStreamPath(juiceStream);
                    break;

                case BananaShower bananaShower:
                    mirrorBananaShower(bananaShower);
                    break;
            }
        }

        /// <summary>
        /// Mirrors the effective X position of <paramref name="catchObject"/> and its nested hit objects.
        /// </summary>
        private static void mirrorEffectiveX(CatchHitObject catchObject)
        {
            catchObject.OriginalX = CatchPlayfield.WIDTH - catchObject.OriginalX;
            catchObject.XOffset = -catchObject.XOffset;

            foreach (var nested in catchObject.NestedHitObjects.Cast<CatchHitObject>())
            {
                nested.OriginalX = CatchPlayfield.WIDTH - nested.OriginalX;
                nested.XOffset = -nested.XOffset;
            }
        }

        /// <summary>
        /// Mirrors the path of the <paramref name="juiceStream"/>.
        /// </summary>
        private static void mirrorJuiceStreamPath(JuiceStream juiceStream)
        {
            var controlPoints = juiceStream.Path.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray();
            foreach (var point in controlPoints)
                point.Position = new Vector2(-point.Position.X, point.Position.Y);

            juiceStream.Path = new SliderPath(controlPoints, juiceStream.Path.ExpectedDistance.Value);
        }

        /// <summary>
        /// Mirrors X positions of all bananas in the <paramref name="bananaShower"/>.
        /// </summary>
        private static void mirrorBananaShower(BananaShower bananaShower)
        {
            foreach (var banana in bananaShower.NestedHitObjects.OfType<Banana>())
                banana.XOffset = CatchPlayfield.WIDTH - banana.XOffset;
        }
    }
}
