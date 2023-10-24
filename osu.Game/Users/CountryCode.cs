// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Users
{
    [JsonConverter(typeof(StringEnumConverter))]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public enum CountryCode
    {
        [Description("Unknown")]
        Unknown = 0,

        [Description("Anonymous Proxy")]
        A1,

        [Description("Satellite Provider")]
        A2,

        [Description("Andorra")]
        AD,

        [Description("United Arab Emirates")]
        AE,

        [Description("Afghanistan")]
        AF,

        [Description("Antigua and Barbuda")]
        AG,

        [Description("Anguilla")]
        AI,

        [Description("Albania")]
        AL,

        [Description("Armenia")]
        AM,

        [Description("Netherlands Antilles")]
        AN,

        [Description("Angola")]
        AO,

        [Description("Asia/Pacific Region")]
        AP,

        [Description("Antarctica")]
        AQ,

        [Description("Argentina")]
        AR,

        [Description("American Samoa")]
        AS,

        [Description("Austria")]
        AT,

        [Description("Australia")]
        AU,

        [Description("Aruba")]
        AW,

        [Description("Aland Islands")]
        AX,

        [Description("Azerbaijan")]
        AZ,

        [Description("Bosnia and Herzegovina")]
        BA,

        [Description("Barbados")]
        BB,

        [Description("Bangladesh")]
        BD,

        [Description("Belgium")]
        BE,

        [Description("Burkina Faso")]
        BF,

        [Description("Bulgaria")]
        BG,

        [Description("Bahrain")]
        BH,

        [Description("Burundi")]
        BI,

        [Description("Benin")]
        BJ,

        [Description("Saint Barthelemy")]
        BL,

        [Description("Bermuda")]
        BM,

        [Description("Brunei")]
        BN,

        [Description("Bolivia")]
        BO,

        [Description("Caribbean Netherlands")]
        BQ,

        [Description("Brazil")]
        BR,

        [Description("Bahamas")]
        BS,

        [Description("Bhutan")]
        BT,

        [Description("Bouvet Island")]
        BV,

        [Description("Botswana")]
        BW,

        [Description("Belarus")]
        BY,

        [Description("Belize")]
        BZ,

        [Description("Canada")]
        CA,

        [Description("Cocos (Keeling) Islands")]
        CC,

        [Description("The Democratic Republic of the Congo")]
        CD,

        [Description("Central African Republic")]
        CF,

        [Description("Congo")]
        CG,

        [Description("Switzerland")]
        CH,

        [Description("Cote D'Ivoire")]
        CI,

        [Description("Cook Islands")]
        CK,

        [Description("Chile")]
        CL,

        [Description("Cameroon")]
        CM,

        [Description("China")]
        CN,

        [Description("Colombia")]
        CO,

        [Description("Costa Rica")]
        CR,

        [Description("Cuba")]
        CU,

        [Description("Cabo Verde")]
        CV,

        [Description("Curaçao")]
        CW,

        [Description("Christmas Island")]
        CX,

        [Description("Cyprus")]
        CY,

        [Description("Czechia")]
        CZ,

        [Description("Germany")]
        DE,

        [Description("Djibouti")]
        DJ,

        [Description("Denmark")]
        DK,

        [Description("Dominica")]
        DM,

        [Description("Dominican Republic")]
        DO,

        [Description("Algeria")]
        DZ,

        [Description("Ecuador")]
        EC,

        [Description("Estonia")]
        EE,

        [Description("Egypt")]
        EG,

        [Description("Western Sahara")]
        EH,

        [Description("Eritrea")]
        ER,

        [Description("Spain")]
        ES,

        [Description("Ethiopia")]
        ET,

        [Description("Europe")]
        EU,

        [Description("Finland")]
        FI,

        [Description("Fiji")]
        FJ,

        [Description("Falkland Islands (Malvinas)")]
        FK,

        [Description("Federated States of Micronesia")]
        FM,

        [Description("Faroe Islands")]
        FO,

        [Description("France")]
        FR,

        [Description("France, Metropolitan")]
        FX,

        [Description("Gabon")]
        GA,

        [Description("United Kingdom")]
        GB,

        [Description("Grenada")]
        GD,

        [Description("Georgia")]
        GE,

        [Description("French Guiana")]
        GF,

        [Description("Guernsey")]
        GG,

        [Description("Ghana")]
        GH,

        [Description("Gibraltar")]
        GI,

        [Description("Greenland")]
        GL,

        [Description("Gambia")]
        GM,

        [Description("Guinea")]
        GN,

        [Description("Guadeloupe")]
        GP,

        [Description("Equatorial Guinea")]
        GQ,

        [Description("Greece")]
        GR,

        [Description("South Georgia and the South Sandwich Islands")]
        GS,

        [Description("Guatemala")]
        GT,

        [Description("Guam")]
        GU,

        [Description("Guinea-Bissau")]
        GW,

        [Description("Guyana")]
        GY,

        [Description("Hong Kong")]
        HK,

        [Description("Heard Island and McDonald Islands")]
        HM,

        [Description("Honduras")]
        HN,

        [Description("Croatia")]
        HR,

        [Description("Haiti")]
        HT,

        [Description("Hungary")]
        HU,

        [Description("Indonesia")]
        ID,

        [Description("Ireland")]
        IE,

        [Description("Israel")]
        IL,

        [Description("Isle of Man")]
        IM,

        [Description("India")]
        IN,

        [Description("British Indian Ocean Territory")]
        IO,

        [Description("Iraq")]
        IQ,

        [Description("Islamic Republic of Iran")]
        IR,

        [Description("Iceland")]
        IS,

        [Description("Italy")]
        IT,

        [Description("Jersey")]
        JE,

        [Description("Jamaica")]
        JM,

        [Description("Jordan")]
        JO,

        [Description("Japan")]
        JP,

        [Description("Kenya")]
        KE,

        [Description("Kyrgyzstan")]
        KG,

        [Description("Cambodia")]
        KH,

        [Description("Kiribati")]
        KI,

        [Description("Comoros")]
        KM,

        [Description("Saint Kitts and Nevis")]
        KN,

        [Description("Democratic People's Republic of Korea")]
        KP,

        [Description("South Korea")]
        KR,

        [Description("Kuwait")]
        KW,

        [Description("Cayman Islands")]
        KY,

        [Description("Kazakhstan")]
        KZ,

        [Description("Lao People's Democratic Republic")]
        LA,

        [Description("Lebanon")]
        LB,

        [Description("Saint Lucia")]
        LC,

        [Description("Liechtenstein")]
        LI,

        [Description("Sri Lanka")]
        LK,

        [Description("Liberia")]
        LR,

        [Description("Lesotho")]
        LS,

        [Description("Lithuania")]
        LT,

        [Description("Luxembourg")]
        LU,

        [Description("Latvia")]
        LV,

        [Description("Libya")]
        LY,

        [Description("Morocco")]
        MA,

        [Description("Monaco")]
        MC,

        [Description("Moldova")]
        MD,

        [Description("Montenegro")]
        ME,

        [Description("Saint Martin")]
        MF,

        [Description("Madagascar")]
        MG,

        [Description("Marshall Islands")]
        MH,

        [Description("North Macedonia")]
        MK,

        [Description("Mali")]
        ML,

        [Description("Myanmar")]
        MM,

        [Description("Mongolia")]
        MN,

        [Description("Macau")]
        MO,

        [Description("Northern Mariana Islands")]
        MP,

        [Description("Martinique")]
        MQ,

        [Description("Mauritania")]
        MR,

        [Description("Montserrat")]
        MS,

        [Description("Malta")]
        MT,

        [Description("Mauritius")]
        MU,

        [Description("Maldives")]
        MV,

        [Description("Malawi")]
        MW,

        [Description("Mexico")]
        MX,

        [Description("Malaysia")]
        MY,

        [Description("Mozambique")]
        MZ,

        [Description("Namibia")]
        NA,

        [Description("New Caledonia")]
        NC,

        [Description("Niger")]
        NE,

        [Description("Norfolk Island")]
        NF,

        [Description("Nigeria")]
        NG,

        [Description("Nicaragua")]
        NI,

        [Description("Netherlands")]
        NL,

        [Description("Norway")]
        NO,

        [Description("Nepal")]
        NP,

        [Description("Nauru")]
        NR,

        [Description("Niue")]
        NU,

        [Description("New Zealand")]
        NZ,

        [Description("Other")]
        O1,

        [Description("Oman")]
        OM,

        [Description("Panama")]
        PA,

        [Description("Peru")]
        PE,

        [Description("French Polynesia")]
        PF,

        [Description("Papua New Guinea")]
        PG,

        [Description("Philippines")]
        PH,

        [Description("Pakistan")]
        PK,

        [Description("Poland")]
        PL,

        [Description("Saint Pierre and Miquelon")]
        PM,

        [Description("Pitcairn")]
        PN,

        [Description("Puerto Rico")]
        PR,

        [Description("State of Palestine")]
        PS,

        [Description("Portugal")]
        PT,

        [Description("Palau")]
        PW,

        [Description("Paraguay")]
        PY,

        [Description("Qatar")]
        QA,

        [Description("Reunion")]
        RE,

        [Description("Romania")]
        RO,

        [Description("Serbia")]
        RS,

        [Description("Russian Federation")]
        RU,

        [Description("Rwanda")]
        RW,

        [Description("Saudi Arabia")]
        SA,

        [Description("Solomon Islands")]
        SB,

        [Description("Seychelles")]
        SC,

        [Description("Sudan")]
        SD,

        [Description("Sweden")]
        SE,

        [Description("Singapore")]
        SG,

        [Description("Saint Helena")]
        SH,

        [Description("Slovenia")]
        SI,

        [Description("Svalbard and Jan Mayen")]
        SJ,

        [Description("Slovakia")]
        SK,

        [Description("Sierra Leone")]
        SL,

        [Description("San Marino")]
        SM,

        [Description("Senegal")]
        SN,

        [Description("Somalia")]
        SO,

        [Description("Suriname")]
        SR,

        [Description("Sao Tome and Principe")]
        ST,

        [Description("El Salvador")]
        SV,

        [Description("Sint Maarten")]
        SX,

        [Description("Syrian Arab Republic")]
        SY,

        [Description("Eswatini")]
        SZ,

        [Description("Turks and Caicos Islands")]
        TC,

        [Description("Chad")]
        TD,

        [Description("French Southern Territories")]
        TF,

        [Description("Togo")]
        TG,

        [Description("Thailand")]
        TH,

        [Description("Tajikistan")]
        TJ,

        [Description("Tokelau")]
        TK,

        [Description("Timor-Leste")]
        TL,

        [Description("Turkmenistan")]
        TM,

        [Description("Tunisia")]
        TN,

        [Description("Tonga")]
        TO,

        [Description("Türkiye")]
        TR,

        [Description("Trinidad and Tobago")]
        TT,

        [Description("Tuvalu")]
        TV,

        [Description("Taiwan")]
        TW,

        [Description("United Republic of Tanzania")]
        TZ,

        [Description("Ukraine")]
        UA,

        [Description("Uganda")]
        UG,

        [Description("United States Minor Outlying Islands")]
        UM,

        [Description("United States")]
        US,

        [Description("Uruguay")]
        UY,

        [Description("Uzbekistan")]
        UZ,

        [Description("Holy See (Vatican City State)")]
        VA,

        [Description("Saint Vincent and the Grenadines")]
        VC,

        [Description("Venezuela")]
        VE,

        [Description("Virgin Islands, British")]
        VG,

        [Description("Virgin Islands, U.S.")]
        VI,

        [Description("Vietnam")]
        VN,

        [Description("Vanuatu")]
        VU,

        [Description("Wallis and Futuna")]
        WF,

        [Description("Samoa")]
        WS,

        [Description("Kosovo")]
        XK,

        [Description("Yemen")]
        YE,

        [Description("Mayotte")]
        YT,

        [Description("South Africa")]
        ZA,

        [Description("Zambia")]
        ZM,

        [Description("Zimbabwe")]
        ZW,
    }
}
