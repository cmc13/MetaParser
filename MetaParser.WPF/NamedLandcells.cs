using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MetaParser.WPF
{
    [TypeConverter(typeof(EnumDisplayConverter))]
    public enum NamedLandcell : uint
    {
        [Display(Name = "Ayan Baqur")] AyanBaqur = 0x1133001F,
        [Display(Name = "Bandit Castle")] BanditCastle = 0xBDD00006,
        [Display(Name = "Underground")] Underground = 0x01E901AD,
        [Display(Name = "Marketplace")] Marketplace = 0x016C01BC,
        [Display(Name = "Ahurenga")] Ahurenga = 0x0FB90009,
        [Display(Name = "Al-Arqas")] Al_Arqas = 0x8F58003B,
        [Display(Name = "Al-Jalima")] Al_Jalima = 0x8588002C,
        [Display(Name = "Arwic")] Arwic = 0xC6A90009,
        [Display(Name = "Baishi")] Baishi = 0xCE410007,
        [Display(Name = "Beach Fort")] BeachFort = 0x42DE000C,
        [Display(Name = "Bluespire")] Bluespire = 0x21B00017,
        [Display(Name = "Cragstone")] Cragstone = 0xBB9F0040,
        [Display(Name = "Dryreach")] Dryreach = 0xDA75002B,
        [Display(Name = "Eastham")] Eastham = 0xCE940035,
        [Display(Name = "Eastwatch")] Eastwatch = 0x49F00013,
        [Display(Name = "Fiun Outpost")] Fiun = 0x38F7001B,
        [Display(Name = "Fort Tethana")] FortTethana = 0x2681001D,
        [Display(Name = "Freehold")] Freehold = 0xF224001A,
        [Display(Name = "Glenden Wood")] GlendenWood = 0xA0A40025,
        [Display(Name = "Greenspire")] Greenspire = 0x2BB5003C,
        [Display(Name = "Hebian-to")] Hebian_to = 0xE64E002F,
        [Display(Name = "Holtburg")] Holtburg = 0xA9B40019,
        [Display(Name = "Kara")] Kara = 0xBA170039,
        [Display(Name = "Khayyaban")] Khayyaban = 0x9F44001A,
        [Display(Name = "Kryst")] Kryst_dsdsfdsf = 0xE822002A,
        [Display(Name = "Lin")] Lin_asdfasf = 0xDC3C0011,
        [Display(Name = "Linvak Tukal")] LinvakTukal = 0xA21E001A,
        [Display(Name = "Lytelthorpe")] Lytelthorpe = 0xC0800007,
        [Display(Name = "Mayoi")] Mayoi = 0xE6320021,
        [Display(Name = "Nanto")] Nanto = 0xE63E0022,
        [Display(Name = "Neydisa Castle")] Neydisa = 0x95D60033,
        [Display(Name = "Danby's Outpost")] Outpost_asdfasfas = 0x5A9C0004,
        [Display(Name = "Plateau Village")] Plateau = 0x49B70021,
        [Display(Name = "Qalabar")] Qalabar = 0x9722003A,
        [Display(Name = "Redspire")] Redspire = 0x17B2002A,
        [Display(Name = "Oolutanga's Refuge")] Refuge = 0xF6820033,
        [Display(Name = "Rithwic")] Rithwic = 0xC98C0028,
        [Display(Name = "Samsur")] Samsur = 0x977B000C,
        [Display(Name = "Sanamar")] Sanamar = 0x33D90015,
        [Display(Name = "Sawato")] Sawato = 0xC95B0001,
        [Display(Name = "Shoushi")] Shoushi_asdfdsafas = 0xDA55001D,
        [Display(Name = "Silyun")] Silyun_asdfdas = 0x26EC003D,
        [Display(Name = "Stonehold")] Stonehold = 0x64D5000B,
        [Display(Name = "Timaru")] Timaru = 0x1DB60016,
        [Display(Name = "Tou-Tou")] Tou_Tou_asdfdas = 0xF5590034,
        [Display(Name = "Town Network")] TownNetwork = 0x00070143,
        [Display(Name = "Tufa")] Tufa = 0x876C0008,
        [Display(Name = "Uziz")] Uziz_asdfas = 0xA260003C,
        [Display(Name = "Westwatch")] Westwatch = 0x23DA002C,
        [Display(Name = "Xarabydun")] Xarabydun = 0x934B0021,
        [Display(Name = "Yanshi")] Yanshi = 0xB46F001E,
        [Display(Name = "Yaraq")] Yaraq = 0x7D64000D,
        [Display(Name = "Zaikhal")] Zaikhal = 0x80900013,
        [Display(Name = "Candeth Keep")] CandethKeep = 0x2B120029,
        [Display(Name = "Crater Lake Village")] CraterLake = 0x90D00107,
        [Display(Name = "Singularity Caul")] SingularityCaul = 0x09040008,
        [Display(Name = "Colosseum")] Colosseum = 0x00AF0118,
        [Display(Name = "Sanctuary")] Sanctuary = 0xF4180104,
        [Display(Name = "Aerlinthe Island")] Aerlinthe = 0xBAE8001D
    }

    public static class NamedLandcellExtensions
    {
        public static bool TryParse(string rep, out NamedLandcell option)
        {
            foreach (var field in typeof(NamedLandcell).GetFields())
            {
                if (field.Name.Equals(rep, StringComparison.OrdinalIgnoreCase))
                {
                    option = (NamedLandcell)field.GetValue(null);
                    return true;
                }
                else
                {
                    var attribute = field.GetCustomAttribute<DisplayAttribute>();
                    if (attribute != null && attribute.Name != null && attribute.Name.Equals(rep, StringComparison.OrdinalIgnoreCase))
                    {
                        option = (NamedLandcell)field.GetValue(null);
                        return true;
                    }
                }
            }

            option = default(NamedLandcell);
            return false;
        }
    }
}
