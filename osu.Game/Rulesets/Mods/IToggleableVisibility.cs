using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public interface IToggleableVisibility
    {
        public void ToggleOffVisibility(Playfield playfield);

        public void ToggleOnVisibility(Playfield playfield);
    }
}