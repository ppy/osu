// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Utils;

namespace osu.Game.Beatmaps.Drawables
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    public partial class BundledBeatmapDownloader : CompositeDrawable
    {
        private readonly bool shouldPostNotifications;

        public IEnumerable<BeatmapDownloadTracker> DownloadTrackers => downloadTrackers;

        private readonly List<BeatmapDownloadTracker> downloadTrackers = new List<BeatmapDownloadTracker>();

        private readonly List<string> downloadableFilenames = new List<string>();

        private BundledBeatmapModelDownloader beatmapDownloader;

        /// <summary>
        /// Construct a new beatmap downloader.
        /// </summary>
        /// <param name="onlyTutorial">Whether only the tutorial should be downloaded, instead of bundled beatmaps.</param>
        /// <param name="shouldPostNotifications">Whether downloads should create tracking notifications.</param>
        public BundledBeatmapDownloader(bool onlyTutorial, bool shouldPostNotifications = false)
        {
            this.shouldPostNotifications = shouldPostNotifications;

            if (onlyTutorial)
            {
                queueDownloads(new[] { tutorial_filename });
            }
            else
            {
                queueDownloads(always_bundled_beatmaps);

                queueDownloads(bundled_osu, 6);
                queueDownloads(bundled_taiko, 3);
                queueDownloads(bundled_catch, 3);
                queueDownloads(bundled_mania, 3);
            }
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var localDependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            localDependencies.CacheAs<BeatmapModelDownloader>(beatmapDownloader = new BundledBeatmapModelDownloader(parent.Get<BeatmapManager>(), parent.Get<IAPIProvider>()));

            if (shouldPostNotifications && parent.Get<INotificationOverlay>() is INotificationOverlay notifications)
                beatmapDownloader.PostNotification = notifications.Post;

            return localDependencies;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (string filename in downloadableFilenames)
            {
                var match = Regex.Match(filename, @"([0-9]*) (.*) - (.*)\.osz");

                var beatmapSet = new APIBeatmapSet
                {
                    OnlineID = int.Parse(match.Groups[1].Value),
                    Artist = match.Groups[2].Value,
                    Title = match.Groups[3].Value,
                };

                var beatmapDownloadTracker = new BeatmapDownloadTracker(beatmapSet);
                downloadTrackers.Add(beatmapDownloadTracker);
                AddInternal(beatmapDownloadTracker);

                // Note that this is downloading the beatmaps even if they are already downloaded.
                // We could rely more on `BeatmapDownloadTracker`'s exposed state to avoid this.
                beatmapDownloader.Download(beatmapSet);
            }
        }

        private void queueDownloads(string[] sourceFilenames, int? limit = null)
        {
            Debug.Assert(LoadState == LoadState.NotLoaded);

            try
            {
                // Matches osu-stable, in order to provide new users with roughly the same randomised selection of bundled beatmaps.
                var random = new LegacyRandom(DateTime.UtcNow.Year * 1000 + (DateTime.UtcNow.DayOfYear / 7));

                downloadableFilenames.AddRange(sourceFilenames.OrderBy(_ => random.NextDouble()).Take(limit ?? int.MaxValue));
            }
            catch { }
        }

        private class BundledBeatmapModelDownloader : BeatmapModelDownloader
        {
            public BundledBeatmapModelDownloader(IModelImporter<BeatmapSetInfo> beatmapImporter, IAPIProvider api)
                : base(beatmapImporter, api)
            {
            }

            protected override ArchiveDownloadRequest<IBeatmapSetInfo> CreateDownloadRequest(IBeatmapSetInfo set, bool minimiseDownloadSize)
                => new BundledBeatmapDownloadRequest(set, minimiseDownloadSize);

            public class BundledBeatmapDownloadRequest : DownloadBeatmapSetRequest
            {
                protected override string Uri => $"https://assets.ppy.sh/client-resources/bundled/{Model.OnlineID}.osz";

                public BundledBeatmapDownloadRequest(IBeatmapSetInfo beatmapSetInfo, bool minimiseDownloadSize)
                    : base(beatmapSetInfo, minimiseDownloadSize)
                {
                }
            }
        }

        /*
         * criteria for bundled maps (managed by pishifat)
         *
         *  auto:
         *  - licensed song
         *  - includes ENHI diffs
         *  - between 60s and 240s
         *
         *  manual:
         *  - bg is explicitly permitted as okay to use. lots of artists say some variation of "it's ok for personal use/non-commercial use/with credit"
         *    (which is prob fine when maps are presented as user-generated content), but for a new osu! player, it's easy to assume bundled maps are
         *     commercial content like other rhythm games, so it's best to be cautious about using not-explicitly-permitted artwork.
         *
         *  - no ai/thirst bgs
         *  - no controversial/explicit song content or titles
         *  - no repeating bundled songs (within each mode)
         *  - no songs that are relatively low production value
         *  - no songs with limited accessibility (annoying high pitch vocals, noise rock, etc)
         */

        private const string tutorial_filename = "1011011 nekodex - new beginnings.osz";

        /// <summary>
        /// Contest winners or other special cases.
        /// </summary>
        private static readonly string[] always_bundled_beatmaps =
        {
            // winner of https://osu.ppy.sh/home/news/2013-09-06-osu-monthly-beatmapping-contest-1
            @"123593 Rostik - Liquid (Paul Rosenthal Remix).osz",
            // winner of https://osu.ppy.sh/home/news/2013-10-28-monthly-beatmapping-contest-2-submissions-open
            @"140662 cYsmix feat. Emmy - Tear Rain.osz",
            // winner of https://osu.ppy.sh/home/news/2013-12-15-monthly-beatmapping-contest-3-submissions-open
            @"151878 Chasers - Lost.osz",
            // winner of https://osu.ppy.sh/home/news/2014-02-14-monthly-beatmapping-contest-4-submissions-now
            @"163112 Kuba Oms - My Love.osz",
            // winner of https://osu.ppy.sh/home/news/2014-05-07-monthly-beatmapping-contest-5-submissions-now
            @"190390 Rameses B - Flaklypa.osz",
            // winner of https://osu.ppy.sh/home/news/2014-09-24-monthly-beatmapping-contest-7
            @"241526 Soleily - Renatus.osz",
            // winner of https://osu.ppy.sh/home/news/2015-02-11-monthly-beatmapping-contest-8
            @"299224 raja - the light.osz",
            // winner of https://osu.ppy.sh/home/news/2015-04-13-monthly-beatmapping-contest-9-taiko-only
            @"319473 Furries in a Blender - Storm World.osz",
            // winner of https://osu.ppy.sh/home/news/2015-06-15-monthly-beatmapping-contest-10-ctb-only
            @"342751 Hylian Lemon - Foresight Is for Losers.osz",
            // winner of https://osu.ppy.sh/home/news/2015-08-22-monthly-beatmapping-contest-11-mania-only
            @"385056 Toni Leys - Dragon Valley (Toni Leys Remix feat. Esteban Bellucci).osz",
            // winner of https://osu.ppy.sh/home/news/2016-03-04-beatmapping-contest-12-osu
            @"456054 IAHN - Candy Luv (Short Ver.).osz",
            // winner of https://osu.ppy.sh/home/news/2020-11-30-a-labour-of-love
            // (this thing is 40mb, I'm not sure if we want it here...)
            @"1388906 Raphlesia & BilliumMoto - My Love.osz",
            // winner of https://osu.ppy.sh/home/news/2022-05-31-triangles
            @"1841885 cYsmix - triangles.osz",
            // winner of https://osu.ppy.sh/home/news/2023-02-01-twin-trials-contest-beatmapping-phase
            @"1971987 James Landino - Aresene's Bazaar.osz",
        };

        private static readonly string[] bundled_osu =
        {
            @"682286 Yuyoyuppe - Emerald Galaxy.osz",
            @"682287 baker - For a Dead Girl+.osz",
            @"682595 baker - Kimi ga Kimi ga -vocanico remix-.osz",
            @"1048705 Thaehan - Never Give Up.osz",
            @"1050185 Carpool Tunnel - Hooked Again.osz",
            @"1052846 Carpool Tunnel - Impressions.osz",
            @"1062477 Ricky Montgomery - Line Without a Hook.osz",
            @"1081119 Celldweller - Pulsar.osz",
            @"1086289 Frums - 24eeev0-$.osz",
            @"1133317 PUP - Free At Last.osz",
            @"1171188 PUP - Full Blown Meltdown.osz",
            @"1177043 PUP - My Life Is Over And I Couldn't Be Happier.osz",
            @"1250387 Circle of Dust - Humanarchy (Cut Ver.).osz",
            @"1255411 Wisp X - Somewhere I'd Rather Be.osz",
            @"1320298 nekodex - Little Drummer Girl.osz",
            @"1323877 Masahiro ""Godspeed"" Aoki - Blaze.osz",
            @"1342280 Minagu feat. Aitsuki Nakuru - Theater Endroll.osz",
            @"1356447 SECONDWALL - Boku wa Boku de shika Nakute.osz",
            @"1368054 SECONDWALL - Shooting Star.osz",
            @"1398580 La priere - Senjou no Utahime.osz",
            @"1403962 m108 - Sunflower.osz",
            @"1405913 fiend - FEVER DREAM (feat. yzzyx).osz",
            @"1409184 Omoi - Hey William (New Translation).osz",
            @"1413418 URBANGARDE - KAMING OUT (Cut Ver.).osz",
            @"1417793 P4koo (NONE) - Sogaikan Utopia.osz",
            @"1428384 DUAL ALTER WORLD - Veracila.osz",
            @"1442963 PUP - DVP.osz",
            @"1460370 Sound Souler - Empty Stars.osz",
            @"1485184 Koven - Love Wins Again.osz",
            @"1496811 T & Sugah - Wicked Days (Cut Ver.).osz",
            @"1501511 Masahiro ""Godspeed"" Aoki - Frostbite (Cut Ver.).osz",
            @"1511518 T & Sugah X Zazu - Lost On My Own (Cut Ver.).osz",
            @"1516617 wotoha - Digital Life Hacker.osz",
            @"1524273 Michael Cera Palin - Admiral.osz",
            @"1564234 P4koo - Fly High (feat. rerone).osz",
            @"1572918 Lexurus - Take Me Away (Cut Ver.).osz",
            @"1577313 Kurubukko - The 84th Flight.osz",
            @"1587839 Amidst - Droplet.osz",
            @"1595193 BlackY - Sakura Ranman Cleopatra.osz",
            @"1667560 xi - FREEDOM DiVE.osz",
            @"1668789 City Girl - L2M (feat. Kelsey Kuan).osz",
            @"1672934 xi - Parousia.osz",
            @"1673457 Boom Kitty - Any Other Way (feat. Ivy Marie).osz",
            @"1685122 xi - Time files.osz",
            @"1689372 NIWASHI - Y.osz",
            @"1729551 JOYLESS - Dream.osz",
            @"1742868 Ritorikal - Synergy.osz",
            @"1757511 KINEMA106 - KARASU.osz",
            @"1778169 Ricky Montgomery - Cabo.osz",
            @"1848184 FRASER EDWARDS - Ruination.osz",
            @"1862574 Pegboard Nerds - Try This (Cut Ver.).osz",
            @"1873680 happy30 - You spin my world.osz",
            @"1890055 A.SAKA - Mutsuki Akari no Yuki.osz",
            @"1911933 Marmalade butcher - Waltz for Chroma (feat. Natsushiro Takaaki).osz",
            @"1940007 Mili - Ga1ahad and Scientific Witchery.osz",
            @"1948970 Shadren - You're Here Forever.osz",
            @"1967856 Annabel - alpine blue.osz",
            @"1969316 Silentroom - NULCTRL.osz",
            @"1978614 Krimek - Idyllic World.osz",
            @"1991315 Feint - Tower Of Heaven (You Are Slaves) (Cut Ver.).osz",
            @"1997470 tephe - Genjitsu Escape.osz",
            @"1999116 soowamisu - .vaporcore.osz",
            @"2010589 Junk - Yellow Smile (bms edit).osz",
            @"2022054 Yokomin - STINGER.osz",
            @"2025686 Aice room - For U.osz",
            @"2035357 C-Show feat. Ishizawa Yukari - Border Line.osz",
            @"2039403 SECONDWALL - Freedom.osz",
            @"2046487 Rameses B - Against the Grain (feat. Veela).osz",
            @"2052201 ColBreakz & Vizzen - Remember.osz",
            @"2055535 Sephid - Thunderstrike 1988.osz",
            @"2057584 SAMString - Ataraxia.osz",
            @"2067270 Blue Stahli - The Fall.osz",
            @"2075039 garlagan - Skyless.osz",
            @"2079089 Hamu feat. yuiko - Innocent Letter.osz",
            @"2082895 FATE GEAR - Heart's Grave.osz",
            @"2085974 HoneyComeBear - Twilight.osz",
            @"2094934 F.O.O.L & Laura Brehm - Waking Up.osz",
            @"2097481 Mameyudoufu - Wave feat. Aitsuki Nakuru.osz",
            @"2106075 MYUKKE. - The 89's Momentum.osz",
            @"2117392 t+pazolite & Komiya Mao - Elustametat.osz",
            @"2123533 LeaF - Calamity Fortune.osz",
            @"2143876 Alkome - Your Voice.osz",
            @"2145826 Sephid - Cross-D Skyline.osz",
            @"2153172 Emiru no Aishita Tsukiyo ni Dai San Gensou Kyoku wo - Eternal Bliss.osz",
        };

        private static readonly string[] bundled_taiko =
        {
            "1048153 Chroma - [@__@].osz",
            "1229307 Venetian Snares - Shaky Sometimes.osz",
            "1236083 meganeko - Sirius A (osu! edit).osz",
            "1248594 Noisia - Anomaly.osz",
            "1272851 siqlo - One Way Street.osz",
            "1290736 Kola Kid - good old times.osz",
            "1318825 SECONDWALL - Light.osz",
            "1320872 MYUKKE. - The 89's Momentum.osz",
            "1337389 cute girls doing cute things - Main Heroine.osz",
            "1397782 Reku Mochizuki - Yorixiro.osz",
            "1407228 II-L - VANGUARD-1.osz",
            "1422686 II-L - VANGUARD-2.osz",
            "1429217 Street - Phi.osz",
            "1442235 2ToneDisco x Cosmicosmo - Shoelaces (feat. Puniden).osz",
            "1447478 Cres. - End Time.osz",
            "1449942 m108 - Crescent Sakura.osz",
            "1463778 MuryokuP - A tree without a branch.osz",
            "1465152 fiend - Fever Dream (feat. yzzyx).osz",
            "1472397 MYUKKE. - Boudica.osz",
            "1488148 Aoi vs. siqlo - Hacktivism.osz",
            "1522733 wotoha - Digital Life Hacker.osz",
            "1540010 Marmalade butcher - Floccinaucinihilipilification.osz",
            "1584690 MYUKKE. - AKKERA-COUNTRY-BOY.osz",
            "1608857 BLOOD STAIN CHILD - S.O.P.H.I.A.osz",
            "1609365 Reku Mochizuki - Faith of Eastward.osz",
            "1622545 METAROOM - I - DINKI THE STARGUIDE.osz",
            "1629336 METAROOM - PINK ORIGINS.osz",
            "1644680 Neko Hacker - Pictures feat. 4s4ki.osz",
            "1650835 RiraN - Ready For The Madness.osz",
            "1661508 PTB10 - Starfall.osz",
            "1671987 xi - World Fragments II.osz",
            "1703065 tokiwa - wasurena feat. Sennzai.osz",
            "1703527 tokiwa feat. Nakamura Sanso - Kotodama Refrain.osz",
            "1704340 A-One feat. Shihori - Magic Girl !!.osz",
            "1712783 xi - Parousia.osz",
            "1718774 Harumaki Gohan - Suisei ni Nareta nara.osz",
            "1719687 EmoCosine - Love Kills U.osz",
            "1733940 WHITEFISTS feat. Sennzai - Paralyzed Ash.osz",
            "1734692 EmoCosine - Cutter.osz",
            "1739529 luvlxckdown - tbh i dont like being social.osz",
            "1756970 Kurubukko vs. yukitani - Minamichita EVOLVED.osz",
            "1762209 Marmalade butcher - Immortality Math Club.osz",
            "1765720 ZxNX - FORTALiCE.osz",
            "1786165 NILFRUITS - Arandano.osz",
            "1787258 SAMString - Night Fighter.osz",
            "1791462 ZxNX - Schadenfreude.osz",
            "1793821 Kobaryo - The Lightning Sword.osz",
            "1796440 kuru x miraie - re:start.osz",
            "1799285 Origami Angel - 666 Flags.osz",
            "1812415 nanobii - Rainbow Road.osz",
            "1814682 NIWASHI - Y.osz",
            "1818361 meganeko - Feral (osu! edit).osz",
            "1818924 fiend - Disconnect.osz",
            "1838730 Pegboard Nerds - Disconnected.osz",
            "1854710 Blaster & Extra Terra - Spacecraft (Cut Ver.).osz",
            "1859322 Hino Isuka - Delightness Brightness.osz",
            "1884102 Maduk - Go (feat. Lachi) (Cut Ver.).osz",
            "1884578 Neko Hacker - People People feat. Nanahira.osz",
            "1897902 uma vs. Morimori Atsushi - Re: End of a Dream.osz",
            "1905582 KINEMA106 - Fly Away (Cut Ver.).osz",
            "1934686 ARForest - Rainbow Magic!!.osz",
            "1963076 METAROOM - S.N.U.F.F.Y.osz",
            "1968973 Stars Hollow - Out the Sunroof..osz",
            "1971951 James Landino - Shiba Paradise.osz",
            "1972518 Toromaru - Sleight of Hand.osz",
            "1982302 KINEMA106 - INVITE.osz",
            "1983475 KNOWER - The Government Knows.osz",
            "2010165 Junk - Yellow Smile (bms edit).osz",
            "2022737 Andora - Euphoria (feat. WaMi).osz",
            "2025023 tephe - Genjitsu Escape.osz",
            "2052754 P4koo - 8th:Planet ~Re:search~.osz",
            "2054122 Raimukun - Myths Orbis.osz",
            "2121470 Raimukun - Nyarlathotep's Dreamland.osz",
            "2122284 Agressor Bunx - Tornado (Cut Ver.).osz",
            "2125034 Agressor Bunx - Acid Mirage (Cut Ver.).osz",
            "2136263 Se-U-Ra - Cris Fortress.osz",
        };

        private static readonly string[] bundled_catch =
        {
            @"693123 yuki. - Nadeshiko Sensation.osz",
            @"833719 FOLiACETATE - Heterochromia Iridis.osz",
            @"981762 siromaru + cranky - conflict.osz",
            @"1008600 LukHash - WHEN AN ANGEL DIES.osz",
            @"1071294 dark cat - pursuit of happiness.osz",
            @"1102115 meganeko - Nova.osz",
            @"1115500 Chopin - Etude Op. 25, No. 12 (meganeko Remix).osz",
            @"1128274 LeaF - Wizdomiot.osz",
            @"1141049 HyuN feat. JeeE - Fallen Angel.osz",
            @"1148215 Zekk - Fluctuation.osz",
            @"1151833 ginkiha - nightfall.osz",
            @"1158124 PUP - Dark Days.osz",
            @"1184890 IAHN - Transform (Original Mix).osz",
            @"1195922 Disasterpeace - Home.osz",
            @"1197461 MIMI - Nanimo nai Youna.osz",
            @"1197924 Camellia feat. Nanahira - Looking For A New Adventure.osz",
            @"1203594 ginkiha - Anemoi.osz",
            @"1211572 MIMI - Lapis Lazuli.osz",
            @"1231601 Lime - Harmony.osz",
            @"1240162 P4koo - 8th:Planet ~Re:search~.osz",
            @"1246000 Zekk - Calling.osz",
            @"1249928 Thaehan - Yuujou.osz",
            @"1258751 Umeboshi Chazuke - ICHIBANBOSHI*ROCKET.osz",
            @"1264818 Umeboshi Chazuke - Panic! Pop'n! Picnic! (2019 REMASTER).osz",
            @"1280183 IAHN - Mad Halloween.osz",
            @"1303201 Umeboshi Chazuke - Run*2 Run To You!!.osz",
            @"1328918 Kobaryo - Theme for Psychopath Justice.osz",
            @"1338215 Lime - Renai Syndrome.osz",
            @"1338796 uma vs. Morimori Atsushi - Re:End of a Dream.osz",
            @"1340492 MYUKKE. - The 89's Momentum.osz",
            @"1393933 Mastermind (xi+nora2r) - Dreadnought.osz",
            @"1400205 m108 - XIII Charlotte.osz",
            @"1471328 Lime - Chronomia.osz",
            @"1503591 Origami Angel - The Title Track.osz",
            @"1524173 litmus* as Ester - Krave.osz",
            @"1541235 Getty vs. DJ DiA - Grayed Out -Antifront-.osz",
            @"1554250 Shawn Wasabi - Otter Pop (feat. Hollis).osz",
            @"1583461 Sound Souler - Absent Color.osz",
            @"1638487 tokiwa - wasurena feat. Sennzai.osz",
            @"1698949 ZxNX - Schadenfreude.osz",
            @"1704324 xi - Time files.osz",
            @"1756405 Fractal Dreamers - Kingdom of Silence.osz",
            @"1769575 cYsmix - Peer Gynt.osz",
            @"1770054 Ardolf - Split.osz",
            @"1772648 in love with a ghost - interdimensional portal leading to a cute place feat. snail's house.osz",
            @"1776379 in love with a ghost - i thought we were lovers w/ basil.osz",
            @"1779476 URBANGARDE - KIMI WA OKUMAGASO.osz",
            @"1789435 xi - Parousia.osz",
            @"1794190 Se-U-Ra - The Endless for Traveler.osz",
            @"1799889 Waterflame - Ricochet Love.osz",
            @"1816401 Gram vs. Yooh - Apocalypse.osz",
            @"1826327 -45 - Total Eclipse of The Sun.osz",
            @"1830796 xi - Halcyon.osz",
            @"1924231 Mili - Nine Point Eight.osz",
            @"1952903 Cres. - End Time.osz",
            @"1970946 Good Kid - Slingshot.osz",
            @"1982063 linear ring - enchanted love.osz",
            @"2000438 Toromaru - Erinyes.osz",
            @"2124277 II-L - VANGUARD-3.osz",
            @"2147529 Nashimoto Ui - AaAaAaAAaAaAAa (Cut Ver.).osz",
        };

        private static readonly string[] bundled_mania =
        {
            @"1008419 BilliumMoto - Four Veiled Stars.osz",
            @"1025170 Frums - We Want To Run.osz",
            @"1092856 F-777 - Viking Arena.osz",
            @"1139247 O2i3 - Heart Function.osz",
            @"1154007 LeaF - ATHAZA.osz",
            @"1170054 Zekk - Fallen.osz",
            @"1212132 Street - Koiyamai (TV Size).osz",
            @"1226466 Se-U-Ra - Elif to Shiro Kura no Yoru -Called-.osz",
            @"1247210 Frums - Credits.osz",
            @"1254196 ARForest - Regret.osz",
            @"1258829 Umeboshi Chazuke - Cineraria.osz",
            @"1300398 ARForest - The Last Page.osz",
            @"1305627 Frums - Star of the COME ON!!.osz",
            @"1348806 Se-U-Ra - LOA2.osz",
            @"1375449 yuki. - Nadeshiko Sensation.osz",
            @"1448292 Cres. - End Time.osz",
            @"1479741 Reku Mochizuki - FORViDDEN ENERZY -Fataldoze-.osz",
            @"1494747 Fractal Dreamers - Whispers from a Distant Star.osz",
            @"1505336 litmus* - Rush-More.osz",
            @"1508963 ARForest - Rainbow Magic!!.osz",
            @"1727126 Chroma - Strange Inventor.osz",
            @"1737101 ZxNX - FORTALiCE.osz",
            @"1740952 Sobrem x Silentroom - Random.osz",
            @"1756251 Plum - Mad Piano Party.osz",
            @"1909163 Frums - theyaremanycolors.osz",
            @"1916285 siromaru + cranky - conflict.osz",
            @"1948972 Ardolf - Split.osz",
            @"1957138 GLORYHAMMER - Rise Of The Chaos Wizards.osz",
            @"1972411 James Landino - Shiba Paradise.osz",
            @"1978179 Andora - Flicker (feat. RANASOL).osz",
            @"1987180 cygnus - The Evolution of War.osz",
            @"1994458 tephe - Genjitsu Escape.osz",
            @"1999339 Aice room - Nyan Nyan Dive (EmoCosine Remix).osz",
            @"2015361 HoneyComeBear - Rainy Girl.osz",
            @"2028108 HyuN - Infinity Heaven.osz",
            @"2055329 miraie & blackwinterwells - facade.osz",
            @"2069877 Sephid - Thunderstrike 1988.osz",
            @"2119716 Aethoro - Snowy.osz",
            @"2120379 Synthion - VIVIDVELOCITY.osz",
            @"2124805 Frums (unknown ""lambda"") - 19ZZ.osz",
            @"2127811 Wiklund - Joy of Living (Cut Ver.).osz",
        };
    }
}
