using System.Linq;
using UnityEngine;
using Game.Levels;
using static Game.Editor.LayoutKit;

namespace Game.Editor
{
    /// <summary>
    /// Room 2 (Kitchen) levels 38–50: the wine glass and burger arrive at 38, ketchup and croissant at 42, drink can and
    /// sandwich at 46. Feasts at 41, 44, 47 and the grand finale at 50, the biggest of all. On two-desk levels only light
    /// food goes up on the back desk, and tall heavy props stay outside its span (x ±0.6).
    /// </summary>
    internal static partial class Room2Layouts
    {
        /// <summary>
        /// New: the wine glass and the burger. Wine glasses on the back desk and the main desk, burger columns at the
        /// sides (a burger pushed off the middle of the back edge would lodge between the desks), a plate of cupcakes.
        /// </summary>
        public static PropPlacement[] BurgersAndGlasses() =>
            Line(PropKind.WineGlass, 0f, -0.3f, 0.3f).Select(OnBack)
                .Concat(Line(PropKind.WineGlass, 0.45f, -0.4f, 0.4f))
                .Concat(new[] { 0.5f, 0.05f }.SelectMany(z => Line(PropKind.Burger, z, -0.8f, 0.8f)))
                .Concat(OnPlates(0f, -0.3f, 1, PropKind.Cupcake, ThreeOnPlate)).ToArray();

        /// <summary>Burgers on plate plinths at the back, bowls with burgers at the front corners, a plate between.</summary>
        public static PropPlacement[] BurgerPlinths() =>
            OnPlates(-0.5f, 0.32f, 1, PropKind.Burger, ThreeOnPlate)
                .Concat(OnPlates(0.5f, 0.32f, 1, PropKind.Burger, ThreeOnPlate))
                .Concat(OnPlates(0f, -0.34f, 1, PropKind.Burger, TwoOnPlate))
                .Concat(new[] { -0.75f, 0.75f }.SelectMany(x => new[] { Food(PropKind.Bowl, x, -0.45f), Food(PropKind.Burger, x, -0.45f, Bowl) }))
                .ToArray();

        /// <summary>Hard: a row of wine glasses and a row of jam jars guard burgers sitting up on jam jars.</summary>
        public static PropPlacement[] GlassGuard() =>
            new[] { -0.6f, 0f, 0.6f }.SelectMany(x => new[] { Food(PropKind.JamJar, x, 0.48f), Food(PropKind.Burger, x, 0.48f, JamJar) })
                .Concat(Line(PropKind.WineGlass, 0f, -0.75f, -0.25f, 0.25f, 0.75f))
                .Concat(Line(PropKind.JamJar, -0.48f, -0.75f, -0.25f, 0.25f, 0.75f)).ToArray();

        /// <summary>Feast: the diner, plates of burgers and of stacked cheese, tomato stacks down the lane and round the back.</summary>
        public static PropPlacement[] Diner() =>
            ThreePlates(PropKind.Burger, ThreeOnPlate, PropKind.Burger, ThreeOnPlate, PropKind.Cheese, FourCheeseOnPlate, backLayers: 2)
                .Concat(Lane.SelectMany(z => Stack(PropKind.Tomato, 3, 0f, z)))
                .Concat(CornerStacks.SelectMany(c => Stack(PropKind.Cheese, 2, c.x, c.y).Append(Food(PropKind.Tomato, c.x, c.y, 2 * Cheese))))
                .Concat(FrontCorners.Select(c => Food(PropKind.Tomato, c.x, c.y))).ToArray();

        /// <summary>New: ketchup and the croissant. Ketchup bottles at the sides, croissants piled in front and on a plate.</summary>
        public static PropPlacement[] KetchupAndCroissants() =>
            OnPlates(0f, 0.3f, 1, PropKind.Croissant, TwoDeepOnPlate)
                .Concat(Line(PropKind.KetchupBottle, 0.55f, -0.85f, -0.55f, 0.55f, 0.85f))
                .Concat(Line(PropKind.KetchupBottle, 0f, -0.75f, 0.75f))
                .Concat(new[] { -0.6f, 0f, 0.6f }.Select(x => Food(PropKind.Croissant, x, -0.45f))).ToArray();

        /// <summary>
        /// Croissants on the back desk (light props only up there), ketchup at the main desk's corners outside its span,
        /// a croissant in a bowl and two loose in between.
        /// </summary>
        public static PropPlacement[] CroissantsOnTheBackDesk() =>
            Line(PropKind.Croissant, 0f, -0.25f, 0.25f).Select(OnBack)
                .Concat(new[] { -0.8f, 0.8f }.SelectMany(x => Line(PropKind.KetchupBottle, 0.45f, x).Concat(Line(PropKind.KetchupBottle, -0.45f, x))))
                .Concat(new[] { Food(PropKind.Bowl, 0f, 0.35f), Food(PropKind.Croissant, 0f, 0.35f, Bowl) })
                .Concat(Line(PropKind.Croissant, 0f, -0.42f, 0.42f)).ToArray();

        /// <summary>Feast: breakfast, plates of cupcakes, a plate of croissants, cupcakes on jam jars all round.</summary>
        public static PropPlacement[] Breakfast() =>
            ThreePlates(PropKind.Cupcake, FiveOnPlate, PropKind.Cupcake, FiveOnPlate, PropKind.Croissant, ThreeDeepOnPlate)
                .Concat(Lane.Select(z => new Vector2(0f, z)).Concat(Corners)
                    .SelectMany(c => new[] { Food(PropKind.JamJar, c.x, c.y), Food(PropKind.Cupcake, c.x, c.y, JamJar) })).ToArray();

        /// <summary>Hard: jam-jar towers at the back, ketchup bottles in front, a row of wine glasses between.</summary>
        public static PropPlacement[] CondimentTowers() =>
            new[] { -0.6f, 0f, 0.6f }.SelectMany(x => Stack(PropKind.JamJar, x == 0f ? 3 : 2, x, 0.45f))
                .Concat(Line(PropKind.KetchupBottle, 0.45f, -0.88f, 0.88f))
                .Concat(Line(PropKind.WineGlass, 0f, -0.75f, -0.25f, 0.25f, 0.75f))
                .Concat(Line(PropKind.KetchupBottle, -0.45f, -0.6f, 0f, 0.6f)).ToArray();

        /// <summary>New: the drink can and the sandwich. Sandwich stacks at the back, two-high cans in front, apples between.</summary>
        public static PropPlacement[] Picnic() =>
            new[] { -0.6f, 0f, 0.6f }.SelectMany(x => Stack(PropKind.Sandwich, 2, x, 0.45f))
                .Concat(new[] { -0.75f, -0.25f, 0.25f, 0.75f }.SelectMany(x => Stack(PropKind.DrinkCan, 2, x, -0.45f)))
                .Concat(Line(PropKind.Apple, 0f, -0.45f, 0.45f)).ToArray();

        /// <summary>Feast: the picnic, sandwich stacks, cheese stacks, two-high cans along the front.</summary>
        public static PropPlacement[] PicnicFeast() =>
            new[] { -0.8f, -0.4f, 0f, 0.4f, 0.8f }.SelectMany(x => Stack(PropKind.Sandwich, 2, x, 0.5f))
                .Concat(new[] { -0.75f, -0.25f, 0.25f, 0.75f }.SelectMany(x => Stack(PropKind.Cheese, 2, x, 0f)))
                .Concat(new[] { -0.45f, -0.15f, 0.15f, 0.45f }.SelectMany(x => Stack(PropKind.DrinkCan, 2, x, -0.52f))).ToArray();

        /// <summary>A two-high can pyramid on the back desk behind sandwich stacks; bowls with sandwiches in front.</summary>
        public static PropPlacement[] CansOnTheBackDesk() =>
            CanPyramid(0f, 0f).Take(6).Select(OnBack)
                .Concat(new[] { -0.6f, 0f, 0.6f }.SelectMany(x => Stack(PropKind.Sandwich, 2, x, 0.45f)))
                .Concat(new[] { -0.5f, 0.5f }.SelectMany(x => new[] { Food(PropKind.Bowl, x, 0f), Food(PropKind.Sandwich, x, 0f, Bowl) }))
                .ToArray();

        /// <summary>Every dish stacked into two dish towers, wine glasses on top, a teapot and glasses in front.</summary>
        public static PropPlacement[] DishTowers() =>
            DishTower(-0.5f, 0.3f, 5).Append(Food(PropKind.WineGlass, -0.5f, 0.3f, DishTowerHeight(5)))
                .Concat(DishTower(0.5f, 0.3f, 5)).Append(Food(PropKind.WineGlass, 0.5f, 0.3f, DishTowerHeight(5)))
                .Append(Food(PropKind.Teapot, 0f, -0.42f))
                .Concat(Line(PropKind.WineGlass, -0.48f, -0.72f, 0.72f)).ToArray();

        /// <summary>
        /// The grand feast finale, the room's biggest: tomato and cheese stacks on the back desk; ketchup, glass and
        /// cupcakes along the back of the main desk; plates of apples and cupcakes; cupcakes and a can down the lane.
        /// Down the middle only small food, which drops through the gap between the desks rather than lodging in it.
        /// </summary>
        public static PropPlacement[] GrandFeast() =>
            new[] { -0.45f, -0.15f, 0.15f, 0.45f }.SelectMany((x, i) => i == 1
                    ? Stack(PropKind.Cheese, 1, x, 0f).Concat(Stack(PropKind.Tomato, 2, x, 0f, Cheese))
                    : Stack(PropKind.Tomato, 3, x, 0f)).Select(OnBack)
                .Concat(new[] { Food(PropKind.KetchupBottle, -0.86f, 0.5f), Food(PropKind.WineGlass, -0.5f, 0.5f), Food(PropKind.KetchupBottle, 0.86f, 0.5f) })
                .Concat(new[] { -0.15f, 0.15f }.Select(x => Food(PropKind.Cupcake, x, 0.5f)))
                .Concat(OnPlates(FrontLeft.x, FrontLeft.y, 1, PropKind.Apple, FiveOnPlate))
                .Concat(OnPlates(FrontRight.x, FrontRight.y, 1, PropKind.Cupcake, FiveOnPlate))
                .Concat(Lane.Select(z => Food(PropKind.Cupcake, 0f, z)))
                .Append(Food(PropKind.DrinkCan, 0.45f, 0.5f))
                .Append(Food(PropKind.Cupcake, 0f, -0.02f))
                .Append(Food(PropKind.Apple, 0.86f, 0.18f)).ToArray();

        private static PropPlacement Lift(PropPlacement placement, float y)
        {
            placement.basePosition.y += y;
            return placement;
        }

        /// <summary>A pyramid of drink cans: four, three on them, then two, centred at (x, z).</summary>
        private static System.Collections.Generic.IEnumerable<PropPlacement> CanPyramid(float x, float z)
        {
            const float pitch = 0.3f;
            for (int row = 0, count = 4; count > 1; row++, count--)
                for (int i = 0; i < count; i++)
                    yield return Food(PropKind.DrinkCan, x + (i - (count - 1) / 2f) * pitch, z, row * DrinkCan);
        }
    }
}
