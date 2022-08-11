using MetaParser.Formatting;

namespace MetaParser.WPF
{
    public static class Formatters
    {
        public static INavReader NavReader { get; } = new DefaultNavReader();
        public static INavWriter NavWriter { get; } = new DefaultNavWriter();
        public static IMetaWriter MetaWriter { get; } = new DefaultMetaWriter(NavWriter);
        public static DefaultMetaReader DefaultMetaReader { get; } = new DefaultMetaReader(NavReader);
        public static IMetaReader XMLMetaReader { get; } = new XMLMetaReader(NavReader);
        public static IMetaReader MetafReader { get; } = new MetafReader();
        public static INavReader MetafNavReader { get; } = new MetafNavReader();
    }
}
