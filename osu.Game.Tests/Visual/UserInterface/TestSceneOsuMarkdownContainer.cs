// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneOsuMarkdownContainer : OsuTestScene
    {
        private OsuMarkdownContainer markdownContainer;

        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Orange);

        [SetUp]
        public void Setup() => Schedule(() =>
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
                    Child = markdownContainer = new OsuMarkdownContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y
                    }
                }
            };
        });

        [Test]
        public void TestEmphases()
        {
            AddStep("Emphases", () =>
            {
                markdownContainer.Text = @"_italic with underscore_
*italic with asterisk*
__bold with underscore__
**bold with asterisk**
*__italic with asterisk, bold with underscore__*
_**italic with underscore, bold with asterisk**_";
            });
        }

        [Test]
        public void TestHeading()
        {
            AddStep("Add Heading", () =>
            {
                markdownContainer.Text = @"# Header 1
## Header 2
### Header 3
#### Header 4
##### Header 5";
            });
        }

        [Test]
        public void TestLink()
        {
            AddStep("Add Link", () =>
            {
                markdownContainer.Text = "[Welcome to osu!](https://osu.ppy.sh)";
            });
        }

        [Test]
        public void TestLinkWithInlineText()
        {
            AddStep("Add Link with inline text", () =>
            {
                markdownContainer.Text = "Hey, [welcome to osu!](https://osu.ppy.sh) Please enjoy the game.";
            });
        }

        [Test]
        public void TestLinkWithTitle()
        {
            AddStep("Add Link with title", () =>
            {
                markdownContainer.Text = "[wikipedia](https://www.wikipedia.org \"The Free Encyclopedia\")";
            });
        }

        [Test]
        public void TestAutoLink()
        {
            AddStep("Add autolink", () =>
            {
                markdownContainer.Text = "<https://discord.gg/ppy>";
            });
        }

        [Test]
        public void TestInlineCode()
        {
            AddStep("Add inline code", () =>
            {
                markdownContainer.Text = "This is `inline code` text";
            });
        }

        [Test]
        public void TestParagraph()
        {
            AddStep("Add paragraph", () =>
            {
                markdownContainer.Text = @"first paragraph

second paragraph

third paragraph";
            });
        }

        [Test]
        public void TestFencedCodeBlock()
        {
            AddStep("Add Code Block", () =>
            {
                markdownContainer.Text = @"```markdown
# Markdown code block

This is markdown code block.
```";
            });
        }

        [Test]
        public void TestSeparator()
        {
            AddStep("Add Seperator", () =>
            {
                markdownContainer.Text = @"Line above

---

Line below";
            });
        }

        [Test]
        public void TestQuote()
        {
            AddStep("Add quote", () =>
            {
                markdownContainer.Text =
                    @"> Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.";
            });
        }

        [Test]
        public void TestTable()
        {
            AddStep("Add Table", () =>
            {
                markdownContainer.Text =
                    @"| Left Aligned  | Center Aligned | Right Aligned |
| :------------------- | :--------------------: | ---------------------:|
| Long Align Left Text | Long Align Center Text | Long Align Right Text |
| Align Left           |      Align Center      |           Align Right |
| Left                 |         Center         |                 Right |";
            });
        }

        [Test]
        public void TestUnorderedList()
        {
            AddStep("Add Unordered List", () =>
            {
                markdownContainer.Text = @"- First item level 1
- Second item level 1
    - First item level 2
        - First item level 3
        - Second item level 3
        - Third item level 3
            - First item level 4
            - Second item level 4
            - Third item level 4
    - Second item level 2
    - Third item level 2
- Third item level 1";
            });
        }

        [Test]
        public void TestOrderedList()
        {
            AddStep("Add Ordered List", () =>
            {
                markdownContainer.Text = @"1. First item level 1
2. Second item level 1
    1. First item level 2
        1. First item level 3
        2. Second item level 3
        3. Third item level 3
            1. First item level 4
            2. Second item level 4
            3. Third item level 4
    2. Second item level 2
    3. Third item level 2
3. Third item level 1";
            });
        }

        [Test]
        public void TestLongMixedList()
        {
            AddStep("Add long mixed list", () =>
            {
                markdownContainer.Text = @"1. The osu! World Cup is a country-based team tournament played on the osu! game mode.
   - While this competition is planned as a 4 versus 4 setup, this may change depending on the number of incoming registrations.
2. Beatmap scoring is based on Score V2.
3. The beatmaps for each round will be announced by the map selectors in advance on the Sunday before the actual matches take place. Only these beatmaps will be used during the respective matches.
   - One beatmap will be a tiebreaker beatmap. This beatmap will only be played in case of a tie. **The only exception to this is the Qualifiers pool.**
4. The match schedule will be settled by the Tournament Management (see the [scheduling instructions](#scheduling-instructions)).
5. If no staff or referee is available, the match will be postponed.
6. Use of the Visual Settings to alter background dim or disable beatmap elements like storyboards and skins are allowed.
7. If the beatmap ends in a draw, the map will be nullified and replayed.
8. If a player disconnects, their scores will not be counted towards their team's total.
   - Disconnects within 30 seconds or 25% of the beatmap length (whichever happens first) after beatmap begin can be aborted and/or rematched. This is up to the referee's discretion.
9. Beatmaps cannot be reused in the same match unless the map was nullified.
10. If less than the minimum required players attend, the maximum time the match can be postponed is 10 minutes.
11. Exchanging players during a match is allowed without limitations.
    - **If a map rematch is required, exchanging players is not allowed. With the referee's discretion, an exception can be made if the previous roster is unavailable to play.**
12. Lag is not a valid reason to nullify a beatmap.
13. All players are supposed to keep the match running fluently and without delays. Penalties can be issued to the players if they cause excessive match delays.
14. If a player disconnects between maps and the team cannot provide a replacement, the match can be delayed 10 minutes at maximum.
15. All players and referees must be treated with respect. Instructions of the referees and tournament Management are to be followed. Decisions labeled as final are not to be objected.
16. Disrupting the match by foul play, insulting and provoking other players or referees, delaying the match or other deliberate inappropriate misbehavior is strictly prohibited.
17. The multiplayer chatrooms are subject to the [osu! community rules](/wiki/Rules).
    - Breaking the chat rules will result in a silence. Silenced players can not participate in multiplayer matches and must be exchanged for the time being.
18. **The seeding method will be revealed after all the teams have played their Qualifier rounds.**
19. Unexpected incidents are handled by the tournament management. Referees may allow higher tolerance depending on the circumstances. This is up to their discretion.
20. Penalties for violating the tournament rules may include:
    - Exclusion of specific players for one beatmap
    - Exclusion of specific players for an entire match
    - Declaring the match as Lost by Default
    - Disqualification from the entire tournament
    - Disqualification from the current and future official tournaments until appealed
    - Any modification of these rules will be announced.";
            });
        }
    }
}
