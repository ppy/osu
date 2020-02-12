// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModCinema<T> : ModCinema, IApplicableToDrawableRuleset<T>
        where T : HitObject
    {
        public virtual void ApplyToDrawableRuleset(DrawableRuleset<T> drawableRuleset)
        {
            drawableRuleset.SetReplayScore(CreateReplayScore(drawableRuleset.Beatmap));

            // AlwaysPresent required for hitsounds
            drawableRuleset.Playfield.AlwaysPresent = true;
            drawableRuleset.Playfield.Hide();
        }
    }

    public class ModCinema : ModAutoplay, IApplicableToHUD, IApplicableToPlayer
    {
        public override string Name => "Cinema";
        public override string Acronym => "CN";
        public override IconUsage? Icon => OsuIcon.ModCinema;
        public override string Description => "Watch the video without visual distractions.";

        public void ApplyToHUD(HUDOverlay overlay)
        {
            overlay.ShowHud.Value = false;
            overlay.ShowHud.Disabled = true;
        }

        public void ApplyToPlayer(Player player)
        {
            player.Background.EnableUserDim.Value = false;

            player.DimmableVideo.IgnoreUserSettings.Value = true;
            player.DimmableStoryboard.IgnoreUserSettings.Value = true;

            player.BreakOverlay.Hide();
        }
    }
}
