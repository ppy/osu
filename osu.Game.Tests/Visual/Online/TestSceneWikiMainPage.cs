// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osu.Game.Overlays.Wiki;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneWikiMainPage : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Orange);

        public TestSceneWikiMainPage()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = overlayColour.Background5,
                    RelativeSizeAxes = Axes.Both,
                },
                new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(20),
                    Child = new WikiMainPage
                    {
                        Markdown = main_page_markdown
                    }
                }
            };
        }

        // From https://osu.ppy.sh/api/v2/wiki/en/Main_page
        private const string main_page_markdown =
            "---\nlayout: main_page\n---\n\n<!-- Do not add any empty lines inside this div. -->\n\n<div class=\"wiki-main-page__blurb\">\nWelcome to the osu! wiki, a project containing a wide range of osu! related information.\n</div>\n\n<div class=\"wiki-main-page__panels\">\n<div class=\"wiki-main-page-panel wiki-main-page-panel--full\">\n\n# Getting started\n\n[Welcome](/wiki/Welcome) • [Installation](/wiki/Installation) • [Registration](/wiki/Registration) • [Help Centre](/wiki/Help_Centre) • [FAQ](/wiki/FAQ)\n\n</div>\n<div class=\"wiki-main-page-panel\">\n\n# Game client\n\n[Interface](/wiki/Interface) • [Options](/wiki/Options) • [Visual settings](/wiki/Visual_Settings) • [Shortcut key reference](/wiki/Shortcut_key_reference) • [Configuration file](/wiki/osu!_Program_Files/User_Configuration_File) • [Program files](/wiki/osu!_Program_Files)\n\n[File formats](/wiki/osu!_File_Formats): [.osz](/wiki/osu!_File_Formats/Osz_(file_format)) • [.osk](/wiki/osu!_File_Formats/Osk_(file_format)) • [.osr](/wiki/osu!_File_Formats/Osr_(file_format)) • [.osu](/wiki/osu!_File_Formats/Osu_(file_format)) • [.osb](/wiki/osu!_File_Formats/Osb_(file_format)) • [.db](/wiki/osu!_File_Formats/Db_(file_format))\n\n</div>\n<div class=\"wiki-main-page-panel\">\n\n# Gameplay\n\n[Game modes](/wiki/Game_mode): [osu!](/wiki/Game_mode/osu!) • [osu!taiko](/wiki/Game_mode/osu!taiko) • [osu!catch](/wiki/Game_mode/osu!catch) • [osu!mania](/wiki/Game_mode/osu!mania)\n\n[Beatmap](/wiki/Beatmap) • [Hit object](/wiki/Hit_object) • [Mods](/wiki/Game_modifier) • [Score](/wiki/Score) • [Replay](/wiki/Replay) • [Multi](/wiki/Multi)\n\n</div>\n<div class=\"wiki-main-page-panel\">\n\n# [Beatmap editor](/wiki/Beatmap_Editor)\n\nSections: [Compose](/wiki/Beatmap_Editor/Compose) • [Design](/wiki/Beatmap_Editor/Design) • [Timing](/wiki/Beatmap_Editor/Timing) • [Song setup](/wiki/Beatmap_Editor/Song_Setup)\n\nComponents: [AiMod](/wiki/Beatmap_Editor/AiMod) • [Beat snap divisor](/wiki/Beatmap_Editor/Beat_Snap_Divisor) • [Distance snap](/wiki/Beatmap_Editor/Distance_Snap) • [Menu](/wiki/Beatmap_Editor/Menu) • [SB load](/wiki/Beatmap_Editor/SB_Load) • [Timelines](/wiki/Beatmap_Editor/Timelines)\n\n[Beatmapping](/wiki/Beatmapping) • [Difficulty](/wiki/Beatmap/Difficulty) • [Mapping techniques](/wiki/Mapping_Techniques) • [Storyboarding](/wiki/Storyboarding)\n\n</div>\n<div class=\"wiki-main-page-panel\">\n\n# Beatmap submission and ranking\n\n[Submission](/wiki/Submission) • [Modding](/wiki/Modding) • [Ranking procedure](/wiki/Beatmap_ranking_procedure) • [Mappers' Guild](/wiki/Mappers_Guild) • [Project Loved](/wiki/Project_Loved)\n\n[Ranking criteria](/wiki/Ranking_Criteria): [osu!](/wiki/Ranking_Criteria/osu!) • [osu!taiko](/wiki/Ranking_Criteria/osu!taiko) • [osu!catch](/wiki/Ranking_Criteria/osu!catch) • [osu!mania](/wiki/Ranking_Criteria/osu!mania)\n\n</div>\n<div class=\"wiki-main-page-panel\">\n\n# Community\n\n[Tournaments](/wiki/Tournaments) • [Skinning](/wiki/Skinning) • [Projects](/wiki/Projects) • [Guides](/wiki/Guides) • [osu!dev Discord server](/wiki/osu!dev_Discord_server) • [How you can help](/wiki/How_You_Can_Help!) • [Glossary](/wiki/Glossary)\n\n</div>\n<div class=\"wiki-main-page-panel\">\n\n# People\n\n[The Team](/wiki/People/The_Team): [Developers](/wiki/People/The_Team/Developers) • [Global Moderation Team](/wiki/People/The_Team/Global_Moderation_Team) • [Support Team](/wiki/People/The_Team/Support_Team) • [Nomination Assessment Team](/wiki/People/The_Team/Nomination_Assessment_Team) • [Beatmap Nominators](/wiki/People/The_Team/Beatmap_Nominators) • [osu! Alumni](/wiki/People/The_Team/osu!_Alumni) • [Project Loved Team](/wiki/People/The_Team/Project_Loved_Team)\n\nOrganisations: [osu! UCI](/wiki/Organisations/osu!_UCI)\n\n[Community Contributors](/wiki/People/Community_Contributors) • [Users with unique titles](/wiki/People/Users_with_unique_titles)\n\n</div>\n<div class=\"wiki-main-page-panel\">\n\n# For developers\n\n[API](/wiki/osu!api) • [Bot account](/wiki/Bot_account) • [Brand identity guidelines](/wiki/Brand_identity_guidelines)\n\n</div>\n<div class=\"wiki-main-page-panel\">\n\n# About the wiki\n\n[Sitemap](/wiki/Sitemap) • [Contribution guide](/wiki/osu!_wiki_Contribution_Guide) • [Article styling criteria](/wiki/Article_Styling_Criteria) • [News styling criteria](/wiki/News_Styling_Criteria)\n\n</div>\n</div>\n";
    }
}
