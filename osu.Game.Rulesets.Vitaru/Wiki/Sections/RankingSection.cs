using osu.Framework.Allocation;
using Symcol.Rulesets.Core.Wiki;

namespace osu.Game.Rulesets.Vitaru.Wiki.Sections
{
    public class RankingSection : WikiSection
    {
        public override string Title => "Map Ranking";

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.Add(new WikiParagraph("The ranking proccess will be very similar to standard at the beginning, where you ask someone qualified to qualify your map and they take a look and either qualify it or tell you why they won't qualify it. (This is of course assuming ruleset creators get to decide how things work, if we don't vitaru simply won't get ranked)"));
            Content.Add(new WikiSubSectionHeader("The Rules"));
            Content.Add(new WikiParagraph("There will be rules of course, although don't expect much as I am one of those people who think aspire level maps are perfectly rankable. In addition to the \"obvious\" stuff like correct timing and acceptable metadata (that usually apply to all maps regardless of ruleset) you're map must meet the following criteria:\n\n" +
                        "Criteria 1: Your map must be passable without getting hit at all. I know converts don't always follow this rule but there isn't much that can be done about that.\n" +
                        "Criteria 2: If your map uses custom bullets they must be checked to be visually accurate enough to an extent without destroying computers.\n" +
                        "Criteria 3: If your map breaks the game todo something awesome, as long as if fits the theme of the song and \"works with it\" then as long as peppy himself doesn't get mad you're probably all set.\n\n" +
                        "Congrats, if your map meets all the above ranking criteria there is a good chance your map can be qualified then ranked! " +
                        "If you would like some friendly advice I would advise you to get as many mods as possible before a \"[Qualification] Review\" as we will call it. " +
                        "If a qualifier has a question about something but sees you have already explained why you think it works and agrees then there will be no need to bring it back up, potentially making the proccess much faster."));
        }
    }
}
