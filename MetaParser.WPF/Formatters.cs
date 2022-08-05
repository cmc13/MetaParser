using MetaParser.Formatting;

namespace MetaParser.WPF
{
    public static class Formatters
    {
        public static readonly INavReader NavReader = new DefaultNavReader();
        public static INavWriter NavWriter = new DefaultNavWriter();
        public static IMetaWriter MetaWriter = new DefaultMetaWriter(NavWriter);
        public static DefaultMetaReader DefaultMetaReader => new DefaultMetaReader(NavReader);
        public static IMetaReader XMLMetaReader => new XMLMetaReader(NavReader);
    }
}
