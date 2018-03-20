using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Vitaru.Settings;
using Symcol.Rulesets.Core.Wiki;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using OpenTK;
using osu.Game.Rulesets.Vitaru.Wiki.Sections.Pieces;
using osu.Framework.Graphics.Shapes;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Rulesets.Vitaru.Scoring;
using osu.Game.Rulesets.Vitaru.Objects.Characters;

namespace osu.Game.Rulesets.Vitaru.Wiki.Sections
{
    public class GameplaySection : WikiSection
    {
        public override string Title => "Gameplay";

        private Bindable<VitaruGamemode> selectedGamemode;
        private Bindable<ScoringMetric> selectedScoring;
        private Bindable<Characters> selectedCharacter;

        private Bindable<bool> familiar;
        private Bindable<bool> lastDance;
        private Bindable<bool> insane;
        private Bindable<bool> awoken;
        private Bindable<bool> sacred;
        private Bindable<bool> resurrected;

        private WikiOptionEnumExplanation<Characters> characterDescription;

        private const string spell_default = "Spell is not implemented yet";

        private Bindable<Mod> selectedMod = new Bindable<Mod> { Default = Mod.Hidden };

        private WikiOptionEnumExplanation<Mod> modsDescription;
        private WikiOptionEnumExplanation<VitaruGamemode> gamemodeDescription;
        private WikiOptionEnumExplanation<ScoringMetric> scoringDescription;

        [BackgroundDependencyLoader]
        private void load()
        {
            selectedGamemode = VitaruSettings.VitaruConfigManager.GetBindable<VitaruGamemode>(VitaruSetting.GameMode);
            selectedScoring = VitaruSettings.VitaruConfigManager.GetBindable<ScoringMetric>(VitaruSetting.ScoringMetric);
            selectedCharacter = VitaruSettings.VitaruConfigManager.GetBindable<Characters>(VitaruSetting.Characters);

            familiar = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.Familiar);
            lastDance = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.LastDance);
            insane = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.Insane);
            awoken = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.Awoken);
            sacred = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.Sacred);
            resurrected = VitaruSettings.VitaruConfigManager.GetBindable<bool>(VitaruSetting.Resurrected);

            Content.Add(new WikiParagraph("Your objective in vitaru is simple, don't get hit by the bullets flying at you, although this is easier said than done."));

            Content.Add(new WikiSubSectionHeader("Converts - Difficulty"));
            Content.Add(new WikiParagraph("The way vitaru converts standard maps to vitaru maps.\n\n" +
                        "Circle Size (CS) affects bullet size.\n" +
                        "Accuracy (OD) affects how large the graze box is / how forgiving the score zones are.\n" +
                        "Health Drain (HP) affects nothing atm (will affect how much damage bullets do to you).\n" +
                        "Approach Rate (AR) affects enemy enter + leave speeds.\n\n" +
                        "Object positions are mapped to the top half of the playfield (or whole playfield for dodge) in the same orientation as standard."));

            Content.Add(new WikiSubSectionHeader("Controls"));
            Content.Add(new WikiParagraph("Controls by default will probably be the most confortable and fitting for all of the gamemodes in this ruleset (if they aren't / weren't they will be changed before release).\n\n" +
                        "W = Move the player up\n" +
                        "S = Down\n" +
                        "A = Left\n" +
                        "D = Right\n" +
                        "Shift = Slow the player to half speed and show the hitbox.\n" +
                        "Space = Speed up to twice as fast (yes, holding space + shift cancles out speed but will still reveal hitbox)\n" +
                        "Left Mouse = Shoot (while in vitaru or touhosu mode)\n" +
                        "Right mouse = Spell (while in touhosu mode)\n\n" +
                        "Some individual character's spells will use additional binds, those will be listed in their spell's description under the \"Characters\" section."));

            Content.Add(new WikiSubSectionHeader("Anatomy"));
            Content.Add(new WikiParagraph("Lets get you familiar with the anatomy of the Player first. " +
                        "Unfortunetly I have not had time to implement squishy insides so for now we are just going to go over the basics.\n"));
            Content.Add(new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,

                Children = new Drawable[]
                {
                    new Anatomy
                    {
                        Position = new Vector2(-20, 0),
                    },
                    new OsuTextFlowContainer(t => { t.TextSize = 20; })
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Width = 400,
                        AutoSizeAxes = Axes.Y,
                        Text = "On the right we have the Player, I also have revealed the hitbox so I can explain why thats the only part that actually matters. " +
                        "First, see that little white dot with the colored ring in the middle of the player? Thats the hitbox. " +
                        "You only take damage if that white part gets hit, bullets will pass right over the rest of the player without actually harming you in any way, infact that heals you!\n"
                    }
                }
            });
            Content.Add(new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,

                Children = new Drawable[]
                {
                    //Just a bullet
                    new CircularContainer
                    {
                        Position = new Vector2(20, 0),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Scale = new Vector2(2),
                        Size = new Vector2(16),
                        BorderThickness = 16 / 4,
                        BorderColour = Color4.Green,
                        Masking = true,

                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Radius = 4,
                            Type = EdgeEffectType.Shadow,
                            Colour = Color4.Green.Opacity(0.5f)
                        }
                    },
                    new OsuTextFlowContainer(t => { t.TextSize = 20; })
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Width = 400,
                        AutoSizeAxes = Axes.Y,
                        Text = "On the left we have a bullet. Bullets are pretty simple, see the white circle in the middle? If that touches the white circle in your hitbox you take damage.\n"
                    }
                }
            });
            Content.Add(new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,

                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Position = new Vector2(-80, 0),
                        Size = new Vector2(200, 40),
                        Masking = true,
                        CornerRadius = 16,
                        BorderThickness = 8,
                        BorderColour = Color4.Aquamarine,

                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    },
                    new OsuTextFlowContainer(t => { t.TextSize = 20; })
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Width = 400,
                        AutoSizeAxes = Axes.Y,
                        Text = "On the right here is a laser. " +
                        "Basically they work like a bullet in that the white rectangle in the middle is the actual dangerous part but unlike a bullet their damage will be spread out for as long as you are getting hit."
                    }
                }
            });
            Content.Add(new WikiSubSectionHeader("Gamemodes"));
            Content.Add(new WikiParagraph("This ruleset has multiple gamemodes built in, similar to how Mania can have different key amounts but instead of just increasing the lanes these change how bullets will be coming at you. " +
                        "What is the same in all 3 of the gamemodes however, is that you will be dodging bullets to the beat to stay alive."));
            Content.Add(gamemodeDescription = new WikiOptionEnumExplanation<VitaruGamemode>(selectedGamemode));
            Content.Add(new WikiSubSectionHeader("Scoring"));
            Content.Add(new WikiParagraph("Scoring done on a per-bullet level and can be done in different ways depending on what you have selected. " +
                        "When vitaru move out of alpha and into beta this will be locked to one metric, the best one."));
            Content.Add(scoringDescription = new WikiOptionEnumExplanation<ScoringMetric>(selectedScoring));
            Content.Add(new WikiSubSectionHeader("Mods"));
            Content.Add(new WikiParagraph("Mods affect gameplay just like the other rulesets in the game, but here is how they affect vitaru so you aren't scratching your head trying to figure it out just by playing with it."));
            Content.Add(modsDescription = new WikiOptionEnumExplanation<Mod>(selectedMod));
            Content.Add(new WikiSubSectionHeader("Characters"));
            Content.Add(new WikiParagraph("Selecting a different character in dodge or vitaru should only change what you look like " +
                "(however I am sure that some parts of Touhosu slip into them at this stage in the ruleset's development). " +
                "In Touhosu however, this will change a number of stats listed below. " +
                "I also listed their " +
                "difficulty to play (Easy, Normal, Hard, Insane, Another, Extra) " +
                "and their Role in a multiplayer setting (Offense, Defense, Support). " +
                "Most of it is subjective but ¯\\_(ツ)_/¯"));
            Content.Add(characterDescription = new WikiOptionEnumExplanation<Characters>(selectedCharacter));

            //basically just an ingame wiki for the characters
            selectedCharacter.ValueChanged += character =>
            {
                string stats = "\nMax Health: " + 100 + "\nMax Energy: " + 100 + "\n" + spell_default;

                restart:
                switch (character)
                {
                    case Characters.ReimuHakurei:
                        stats = "\nMax Health: 100" +
                        "\nMax Energy: 30" +
                        "\nRole: Offense" +
                        "\nDifficulty: Easy" +
                        "\nSpell (10 energy): Rune-Seal (Not Implemented)";

                        if (selectedGamemode.Value == VitaruGamemode.Touhosu)
                        {
                            stats = stats + "\n\nReimu used to be a complete air head. " +
                        "But time and hardship has shaped her into the strong cunning magician she is today. " +
                        "Usually you would be hard-pressed to not only get the jump on her but even find her before she finds you. " +
                        "However don't let this fool you, she is by no means a rutheless killer like some of her friends, infact she is quite sweet. " +
                        "Just try not to get on her bad side. ";

                            if (!familiar)
                                stats = stats +
                            "She seems to be spacing out again lately though, almost like she is off in her own world. . .\n\n" +
                            "And indeed she is, dreaming of a night long gone.";
                        }
                        break;
                    case Characters.MarisaKirisame:
                        stats = "\nMax Health: 100" +
                        "\nMax Energy: 30" +
                        "\nRole: Offense" +
                        "\nDifficulty: Easy" +
                        "\nSpell (10 energy): Mini-Hakkero (WIP)";

                        if (selectedGamemode.Value == VitaruGamemode.Touhosu && familiar)
                            stats = stats + "\n\nMarisa Kirisame, the magical witch of the forest who could do no wrong, or so they said. " +
                        "One thing that is certain is her lust for control, she never lets the situation get out of hand. " +
                        "Last time she did it cost her greatly, and created wounds that won't heal as easily as she would lead you to believe.";
                        break;
                    case Characters.SakuyaIzayoi:
                        stats = "\nMax Health: 100" +
                        "\nMax Energy: 36" +
                        "\nRole: Defense" +
                        "\nDifficulty: Normal" +
                        "\nSpell (6 energy, 3 per second): Time-Warden";

                        if (selectedGamemode.Value == VitaruGamemode.Touhosu)
                            stats = stats + "\n\nYoung Sakuya used to be kind and caring for all, like the ones who raised her. " +
                        "But even the purest of hearts can be broken given the right circumstances, corrupted by the dark things that lurk in the night. " +
                        "Physical wounds may heal, but the emotional stabbing she was subjected to can never be mended. " +
                        "Now she spends every ounce of willpower to keep to her schedule, everything must be timed perfectly.";
                        break;
                    case Characters.HongMeiling:
                        stats = "\nMax Health: 0 (when resurrected 20)" +
                        "\nMax Energy: 36" +
                        "\nRole: Defense" +
                        "\nDifficulty: Time Freeze" +
                        "\nAbility (passive): Leader (WIP)";

                        if (false)//selectedGamemode.Value == VitaruGamemode.Touhosu)
                            stats = stats + "\n\nHong was your typical war hero. She fought valiantly, saved allies, showed no mercy against the enemy. " +
                        "She didn't really care for all the medals or attention though, now that the war was over she just wanted to retire to her mansion.\n\n" +
                        "Upon returning home she met with the Scarlet sisters she had entrusted the house with, ony to find they are different. " +
                        "They had wings, they grew wings? They were fairies now? " +
                        "No, Remilia was a vampire now, Flan is a Fairy." +
                        "Hong didn't mind as they were still dear friends and dispite having a track record of mishieve, the fact that the mansion was still standing was a testement to their capabilities.\n\n" +
                        "Thats not all though, they also had a small child with them, she couldn't have been older than two or three years old. " +
                        "She had grey eyes and grey hair, they dressed her in Hong's old maid uniform, the blue one, and begun to teach her how to keep the place in order. " +
                        "Hong hadn't said a word yet, and didn't. " +
                        "She went straight to her room and propmtly fell asleep on her bed, which was just as soft as when she left it.\n\n" +
                        "She awoke to the small child holding a tea set at her bed side. " +
                        "She stood straight as a pencil, proper maid edicate.\n\n" +
                        "Hong reached for one of the tea cups that had already been poured next to a note that read: \"Hope you still like 'Green Moon'. -Remilia\"\n" +
                        "She took a sip then began: \"Thank you, how long have you been here?\"\n\n" +
                        "The little girl who sounded almost too fragile hesitated, she didn't expect anything to be spoken today: \"A few months? Its hard to keep track of time inside all day.\"\n\n" +
                        "\"Huh, they never let you go outside?\" Hong said taking the tray from the little girl.\n\n" +
                        "\"No, they said those jobs are too hard for me because I am still little, but sometimes I finish early and watch them through the windows. Cleaning the fountain doesn't look that hard.\"\n\n" +
                        "\"Come with me.\" she said finaly standing up and walking out into the hallway. " +
                        "She first went to the kitchen to drop off the tray then took the young girl out back. " +
                        "The Scarlet sisters where halfway through redoing the roofing and were up of the roof working on it. " +
                        "They exchanged waves with Hong and the girl then went back to working, it appeared as they had no struggle with it as they could fly now.\n\n" +
                        "\"I don't believe I have asked your name yet, do you mind sharing?\"\n\n" +
                        "\"The two sisters call me Sakeuta, apparently it means 'time waster' in some old language?\"\n\n" +
                        "\"Why would they say that?\"\n\n" +
                        "\"Sometimes I slack on cleaning dishes, it is just soo boring.\"\n\n" +
                        "\"It certainly is, but that will never do for a name.\" " +
                        "Hong was thinking, then she saw her staring at the shedding cherry blossoms over by the maze.\n\n" +
                        "\"How about Sakuya?\"\n\n" +
                        "\"Huh?\"\n\n" +
                        "\"Your name? How about Sakuya. It means 'time warden' in an old arcane language I studied in my free time.\"\n\n" +
                        "\"Hmm, its better than 'time waster', I like it.\"\n\n" +
                        "Hong was wondering where Sakuya came from, and would question the Scarlet sisters later about. " +
                        "For now though, she was transfixed on how similar she is to Hong was when she was little, slightly defient but eager to help.\n\n" +
                        "\"Let me show you a few tricks to the jobs back here I bet the Scarlet sisters don't know.\"";
                        break;
                    case Characters.FlandreScarlet:
                        stats = "\nMax Health: 100" +
                        "\nMax Energy: 80" +
                        "\nRole: Offense" +
                        "\nDifficulty: Easy" +
                        "\nSpell (40 energy): Taboo";

                        if (selectedGamemode.Value == VitaruGamemode.Touhosu && familiar)
                            stats = stats + "\n\nFlandre used to be one of the most feared fairies around, and thats no small feat. " +
                        "Fairies are have a tendency to be stupid, but Flandre wasn't always a Fairy now was she?\n\n" +
                        "Now days all she does is mindless games in the basement, broken but not lost. " +
                        "One day she could return, and you wouldn't want to be on the recieving end of her wrath.";
                        break;
                    case Characters.RemiliaScarlet:
                        stats = "\nMax Health: 60" +
                        "\nMax Energy: 60" +
                        "\nRole: Offense" +
                        "\nDifficulty: Normal" +
                        "\nAbility (passive / 0.5 health per hit): Vampuric";

                        if (selectedGamemode.Value == VitaruGamemode.Touhosu && familiar)
                            stats = stats + "\n\nRemilia wasn't always a vampire, she didn't always have a thirst for blood. " +
                        "But things change, something happened one day and she was 'ascended' she keeps telling herself with her sister. " +
                        "Certainly this was a change for the better though, after all biological imortality is hard to come by.\n\n" +
                        "She also loves her sister Flandre dearly, but for a long time now she has been broken. " +
                        "Flan always used to be a bit loopy but now she won't even speak to Remilia anymore." +
                        "All she does is dwell in the basement all day playing make believe games alone in the dark. " +
                        "Perhaps one day she may be put back together, however unlikely.\n\n" +
                        "For now this family shall remain shattered by un-imaginable pain, but the worst has yet to come.";
                        break;
                    case Characters.Cirno:
                        stats = "\nMax Health: 80" +
                        "\nMax Energy: 40" +
                        "\nRole: Defense" +
                        "\nDifficulty: Easy" +
                        "\nAbility (40 energy): Shatter";
                        break;
                    case Characters.YuyukoSaigyouji:
                        stats = "\nMax Health: 100" +
                        "\nMax Energy: 20" +
                        "\nRole: Defense" +
                        "\nDifficulty: Normal" +
                        "\nSpell (4 energy, 2 per second): Ghastly Dream";
                        break;
                    case Characters.YukariYakumo:
                        stats = "\nMax Health: 80" +
                        "\nMax Energy: 24" +
                        "\nRole: Support" +
                        "\nDifficulty: Another" +
                        "\nAbility (4 energy, 4 per second): Rift (Buggy?)";

                        if (false)//selectedGamemode.Value == VitaruGamemode.Touhosu)
                            stats = stats + "\n\nThere are many stories about Yukari, some say she was born of some rich folk to the far west and some say she predates the known universe itself. " +
                        "While that would explain her unatural abilities in combat they would not explain her uncanny abitlity to empithize with her \"creations\". " +
                        "The only other individual to supposedly be even relativly this old is cold and heartless (perhaps litterally).";
                        break;
                    case Characters.SikieikiYamaxanadu:
                        stats = "\nMax Health: 80" +
                        "\nMax Energy: 40" +
                        "\nRole: Offense + Defense" +
                        "\nDifficulty: ???" +
                        "\nAbility (2 stab, 4 per second of block, 6 swipe, 10 wipe): Judgement (Not Implemented)";
                        break;
                    case Characters.KokoroHatano:
                        stats = "\nMax Health: 100" +
                        "\nMax Energy: 36" +
                        "\nRole: Offense + Defense" +
                        "\nDifficulty: Extra" +
                        "\nAbility (passive): Last Dance (Buggy?)";
                        break;
                    case Characters.Kaguya:
                        stats = "\nMax Health: 80" +
                        "\nMax Energy: 36" +
                        "\nRole: Support" +
                        "\nDifficulty: Hard" +
                        "\nSpell (4 energy): Lunar Shift (Not Implemented)";
                        break;
                    case Characters.IbarakiKasen:
                        stats = "\nMax Health: 40" +
                        "\nMax Energy: 8" +
                        "\nRole: Offense" +
                        "\nDifficulty: Insane" +
                        "\nSpell (2 energy): Blink (Pending New Spell)";
                        break;
                    case Characters.NueHoujuu:
                        stats = "\nMax Health: 80" +
                        "\nMax Energy: 24" +
                        "\nRole: Support" +
                        "\nDifficulty: Another" +
                        "\nSpell (Ratio [energy:damage/energy/health/weaken] - 1:4/2/1/2): Invasion (WIP)";
                        break;
                    case Characters.Rock:
                        stats = "\nMax Health: 20" +
                        "\nMax Energy: 0" +
                        "\nRole: *Silence*" +
                        "\nDifficulty: *More Silence*" +
                        "\nSpell (0 energy): Death by Stoning";
                        break;
                    case Characters.AliceMuyart:
                        if (!VitaruAPIContainer.Shawdooow)
                        {
                            selectedCharacter.Value = Characters.ReimuHakurei;
                            character = Characters.ReimuHakurei;
                            goto restart;
                        }
                        stats = "\nMax Health: 200 (x2 Healing)" +
                        "\nMax Energy: 200 (x2 Gain)" +
                        "\nRole: Offense" +
                        "\nDifficulty: Hard" +
                        "\nSpell: UnNatural";
                        break;
                    case Characters.ArysaMuyart:
                        if (!VitaruAPIContainer.Shawdooow)
                        {
                            selectedCharacter.Value = Characters.ReimuHakurei;
                            character = Characters.ReimuHakurei;
                            goto restart;
                        }
                        stats = "\nMax Health: 60" +
                        "\nMax Energy: 80" +
                        "\nRole: Defense" +
                        "\nDifficulty: ???" +
                        "\nSpell: Seasonal Shift";
                        break;
                }

                characterDescription.Description.Text = stats;
            };
            selectedCharacter.TriggerChange();

            selectedGamemode.ValueChanged += gamemode =>
            {
                switch (gamemode)
                {
                    case VitaruGamemode.Vitaru:
                        gamemodeDescription.Description.Text = "The default gamemode in this ruleset which is based on the touhou series danmaku games. " +
                        "Allows you to kill enemies while dodging bullets to the beat!";
                        break;
                    case VitaruGamemode.Dodge:
                        gamemodeDescription.Description.Text = "Completly changes how vitaru is played. " +
                        "The Dodge gamemode changes the playfield to a much shorter rectangle and send bullets your way from all directions while also taking away your ability to shoot!";
                        break;
                    case VitaruGamemode.Touhosu:
                        gamemodeDescription.Description.Text = "The \"amplified\" gamemode. Touhosu mode is everything Vitaru is and so much more. " +
                        "Selecting different characters no longer just changes your skin but also your stats and allows you to use spells!\n\n" +
                        "Also allows you to start story mode.";
                        break;
                }
                selectedCharacter.TriggerChange();
            };
            selectedGamemode.TriggerChange();

            selectedScoring.ValueChanged += scoring =>
            {
                switch (scoring)
                {
                    case ScoringMetric.Graze:
                        scoringDescription.Description.Text = "Score per bullet is based on how close it got to hitting you, the closer a bullet got the more score it will give.";
                        break;
                    case ScoringMetric.ScoreZones:
                        scoringDescription.Description.Text = "Based on where you are located on the screen, the closer to the center the more score you will get.";
                        break;
                    case ScoringMetric.InverseCatch:
                        scoringDescription.Description.Text = "Quite litterally the opposite of catch, if a bullet doesn't hit you its a Perfect";
                        break;
                }
            };
            selectedScoring.TriggerChange();

            selectedMod.ValueChanged += mod =>
            {
                switch (mod)
                {
                    default:
                        modsDescription.Description.Text = "Check back later!";
                        break;
                    case Mod.Easy:
                        modsDescription.Description.Text = "Bullets are smaller (Singleplayer only),\n" +
                        "You deal more damage (Multiplayer only),\n" +
                        "You take less damage,\n" +
                        "In Touhosu mode you will generate energy faster.";
                        break;
                    case Mod.Hidden:
                        modsDescription.Description.Text = "Bullets fade out over time";
                        break;
                    case Mod.Flashlight:
                        modsDescription.Description.Text = "Bullets are only visable near you";
                        break;
                    case Mod.HardRock:
                        modsDescription.Description.Text = "You deal less damage (Singleplayer only),\n" +
                        "Your Hitbox is larger,\n" +
                        "You take more damage,\n" +
                        "In Touhosu mode you will generate energy slower.";
                        break;
                }
            };
            selectedMod.TriggerChange();
        }
    }

    public enum Mod
    {
        Easy,

        HardRock,
        Hidden,
        Flashlight,
        SuddenDeath,
        Perfect,

        Relax
    }
}
