// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace osu.Game.Localisation
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public enum Language
    {
        [Description(@"English")]
        en,

        // TODO: Requires Arabic glyphs to be added to resources (and possibly also RTL support).
        // [Description(@"اَلْعَرَبِيَّةُ")]
        // ar,

        [Description(@"беларуская мова")]
        be,

        [Description(@"български")]
        bg,

        [Description(@"català")]
        ca,

        [Description(@"česky")]
        cs,

        [Description(@"dansk")]
        da,

        [Description(@"Deutsch")]
        de,

        [Description(@"ελληνικά")]
        el,

        [Description(@"español")]
        es,

        // TODO: Requires Arabic glyphs to be added to resources (and possibly also RTL support).
        // [Description(@"فارسی")]
        // fa_ir,

        [Description(@"suomi")]
        fi,

        [Description(@"wikang Filipino")]
        fil,

        [Description(@"français")]
        fr,

        // TODO: Requires Hebrew glyphs to be added to resources (and possibly also RTL support).
        // [Description(@"עברית")]
        // he,

        [Description(@"Hrvatski")]
        hr_hr,

        [Description(@"magyar")]
        hu,

        [Description(@"bahasa Indonesia")]
        id,

        [Description(@"italiano")]
        it,

        [Description(@"日本語")]
        ja,

        [Description(@"한국어")]
        ko,

        [Description(@"lietuvių kalba")]
        lt,

        [Description(@"Latviešu")]
        lv_lv,

        [Description(@"Melayu")]
        ms_my,

        [Description(@"Nederlands")]
        nl,

        [Description(@"norsk")]
        no,

        [Description(@"polski")]
        pl,

        [Description(@"português")]
        pt,

        [Description(@"português brasileiro")]
        pt_br,

        [Description(@"română")]
        ro,

        [Description(@"русский")]
        ru,

        // TODO: Requires Sinhala glyphs to be added to resources.
        // Additionally, no translations available yet.
        // [Description(@"සිංහල")]
        // si_lk,

        [Description(@"slovenčina")]
        sk,

        [Description(@"slovenščina")]
        sl,

        [Description(@"српски")]
        sr,

        [Description(@"svenska")]
        sv,

        // Tajik has no associated localisations yet, and is not supported on Windows versions <10.
        // TODO: update language mapping in osu-resources to redirect tg-TJ to tg-Cyrl-TJ (which is supported on earlier Windows versions)
        // [Description(@"Тоҷикӣ")]
        // tg_tj,

        [Description(@"ไทย")]
        th,

        [Description(@"Türkçe")]
        tr,

        [Description(@"українська мова")]
        uk,

        [Description(@"tiếng Việt")]
        vi,

        [Description(@"简体中文")]
        zh,

        // Traditional Chinese (Hong Kong) is listed in web sources but has no associated localisations,
        // and was wrongly falling back to Simplified Chinese.
        // Can be revisited if localisations ever arrive.
        // [Description(@"繁體中文（香港）")]
        // zh_hk,

        [Description(@"繁體中文（台灣）")]
        zh_hant,

#if DEBUG
        [Description(@"Debug (show raw keys)")]
        debug
#endif
    }
}
