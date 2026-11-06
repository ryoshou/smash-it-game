namespace Game.Levels
{
    /// <summary>Every spawnable desk prop. Level assets store this as a number: append new kinds, never reorder.</summary>
    public enum PropKind
    {
        Keycap1u,
        Keycap2u,
        KeycapSpace,
        KeycapEnter,
        Mug,
        Book,
        CactusPot,
        PaperCup,
        DeskLamp,
        // M4a, Café (levels 51–75)
        Plate,
        Donut,
        MilkBottle,
        // M4a, Library (levels 76–100)
        Candle,
        Hourglass,
        PencilCup,
        // M4a, Gaming Den (levels 101–125)
        SodaCan,
        GameController,
        FizzySoda,
        // Kitchen (levels 26–50; Kitchen spec §5): dishes shatter, food splashes, all true to size
        DinnerPlate,
        Bowl,
        Teapot,
        WineGlass,
        JamJar,
        KetchupBottle,
        DrinkCan,
        Apple,
        Tomato,
        Cheese,
        Burger,
        Cupcake,
        Croissant,
        Sandwich,
    }
}
