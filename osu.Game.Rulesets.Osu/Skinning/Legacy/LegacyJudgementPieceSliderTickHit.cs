// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public partial class LegacyJudgementPieceSliderTickHit : Sprite, IAnimatableJudgement
    {
        public void PlayAnimation()
        {
            // https://github.com/peppy/osu-stable-reference/blob/0e91e49bc83fe8b21c3ba5f1eb2d5d06456eae84/osu!/GameModes/Play/Rulesets/Ruleset.cs#L804-L806
            this.MoveToOffset(new Vector2(0, -10), 300, Easing.Out)
                .Then()
                .FadeOut(60);
        }

        public Drawable GetAboveHitObjectsProxiedContent() => CreateProxy();
    }
}
