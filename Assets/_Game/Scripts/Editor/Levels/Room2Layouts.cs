using System.Collections.Generic;
using System.Linq;
using Game.Levels;
using UnityEngine;
using static Game.Editor.LayoutKit;

namespace Game.Editor
{
    /// <summary>
    /// Room 2 (Kitchen) layouts, levels 26–37 (Kitchen spec §4): the dinner plate and apple arrive at 26, bowl and
    /// tomato at 28, teapot and cheese at 32, jam jar and cupcake at 36. Feasts (27, 31, 34, 37) heap food on plates and
    /// stack the box-shaped foods; they are a reward, never tall towers. Every prop is a target; 30 and 35 are Hard.
    /// </summary>
    internal static partial class Room2Layouts
    {
        // Spots on a 0.8-wide dinner plate (or a bowl's rim) for food of each size, as offsets from its centre.
        private static readonly Vector2[] FourOnPlate = { new Vector2(-0.17f, -0.17f), new Vector2(0.17f, -0.17f), new Vector2(-0.17f, 0.17f), new Vector2(0.17f, 0.17f) };
        private static readonly Vector2[] FiveOnPlate = { new Vector2(-0.2f, -0.2f), new Vector2(0.2f, -0.2f), Vector2.zero, new Vector2(-0.2f, 0.2f), new Vector2(0.2f, 0.2f) };
        private static readonly Vector2[] ThreeOnPlate = { new Vector2(0f, 0.2f), new Vector2(-0.18f, -0.11f), new Vector2(0.18f, -0.11f) };
        private static readonly Vector2[] TwoOnPlate = { new Vector2(-0.18f, 0f), new Vector2(0.18f, 0f) };
        private static readonly Vector2[] TwoDeepOnPlate = { new Vector2(0f, -0.13f), new Vector2(0f, 0.13f) };
        private static readonly Vector2[] FourCheeseOnPlate = { new Vector2(-0.13f, -0.19f), new Vector2(0.13f, -0.19f), new Vector2(-0.13f, 0.19f), new Vector2(0.13f, 0.19f) };
        private static readonly Vector2[] OnCentre = { Vector2.zero };
        // A feast's frame: two plates in front and one at the back centre (0.8 plates don't fit two deep on a 1.5 desk),
        // a lane between the front plates, small spots in the back corners and a wider spot (a cheese stack) by each.
        private static readonly Vector2 FrontLeft = new Vector2(-0.58f, -0.34f);
        private static readonly Vector2 FrontRight = new Vector2(0.58f, -0.34f);
        private static readonly Vector2 BackCentre = new Vector2(0f, 0.34f);
        private static readonly float[] Lane = { -0.6f, -0.31f };
        private static readonly Vector2[] Corners = { new Vector2(-0.8f, 0.58f), new Vector2(0.8f, 0.58f), new Vector2(-0.8f, 0.2f), new Vector2(0.8f, 0.2f) };
        private static readonly Vector2[] CornerStacks = { new Vector2(-0.8f, 0.47f), new Vector2(0.8f, 0.47f) };
        private static readonly Vector2[] FrontCorners = { new Vector2(-0.86f, 0.16f), new Vector2(0.86f, 0.16f) };
        private static readonly Vector2[] ThreeDeepOnPlate = { new Vector2(0f, -0.26f), Vector2.zero, new Vector2(0f, 0.26f) };

        /// <summary>New: the dinner plate and the apple. Apples heaped on two plates at the back, two more between them.</summary>
        public static PropPlacement[] ApplePlates() =>
            OnPlates(-0.55f, 0.34f, 1, PropKind.Apple, FourOnPlate)
                .Concat(OnPlates(0.55f, 0.34f, 1, PropKind.Apple, FourOnPlate))
                .Concat(new[] { Food(PropKind.Apple, 0f, 0.55f), Food(PropKind.Apple, 0f, 0.22f) }).ToArray();

        /// <summary>
        /// Feast: a fruit spread end to end, three plates heaped with apples, apples down the lane and round the back
        /// plate, paper cups in the back corners (the mug is too wide for them, and an apple rocks on a cup's lid).
        /// </summary>
        public static PropPlacement[] FruitSpread() =>
            ThreePlates(PropKind.Apple, FiveOnPlate, PropKind.Apple, FiveOnPlate, PropKind.Apple, FiveOnPlate)
                .Concat(Lane.Select(z => Food(PropKind.Apple, 0f, z)))
                .Concat(Corners.Select((c, i) => Food(i < 2 ? PropKind.PaperCup : PropKind.Apple, c.x, c.y)))
                .Concat(Line(PropKind.Apple, 0.62f, -0.48f, 0.48f)).ToArray();

        /// <summary>New: the bowl and the tomato. Three bowls of tomatoes at the back, two in front, one tomato loose.</summary>
        public static PropPlacement[] TomatoBowls() =>
            new[] { -0.6f, 0f, 0.6f }.SelectMany(x => InBowl(x, 0.4f, 2))
                .Concat(InBowl(-0.3f, -0.35f, 1)).Concat(InBowl(0.3f, -0.35f, 1))
                .Append(Food(PropKind.Tomato, 0.82f, -0.45f)).ToArray();

        /// <summary>Apples along the back desk; bowls of tomatoes at the sides and a plate of apples in front.</summary>
        public static PropPlacement[] ApplesOnTheBackDesk() =>
            Line(PropKind.Apple, 0f, -0.45f, -0.15f, 0.15f, 0.45f).Select(OnBack)
                .Concat(InBowl(-0.7f, 0.2f, 2)).Concat(InBowl(0.7f, 0.2f, 2))
                .Concat(OnPlates(0f, 0.25f, 1, PropKind.Apple, ThreeOnPlate)).ToArray();

        /// <summary>Hard: two plate-and-bowl towers, each crowned with fruit, and two apples guarding the front.</summary>
        public static PropPlacement[] CrownedTowers() =>
            DishTower(-0.5f, 0.15f, 4).Append(Food(PropKind.Tomato, -0.5f, 0.15f, DishTowerHeight(4)))
                .Concat(DishTower(0.5f, 0.15f, 4)).Append(Food(PropKind.Tomato, 0.5f, 0.15f, DishTowerHeight(4)))
                .Concat(Line(PropKind.Apple, -0.5f, -0.2f, 0.2f)).ToArray();

        /// <summary>Feast: the salad bar, six bowls piled with tomatoes and a row of apples along the front.</summary>
        public static PropPlacement[] SaladBar() =>
            new[] { -0.65f, 0f, 0.65f }.SelectMany(x => InBowl(x, 0.4f, 3))
                .Concat(new[] { -0.65f, 0f, 0.65f }.SelectMany(x => InBowl(x, -0.15f, 3)))
                .Concat(Line(PropKind.Apple, -0.6f, -0.8f, -0.48f, -0.16f, 0.16f, 0.48f, 0.8f)).ToArray();

        /// <summary>New: the teapot and the cheese. The teapot on a plate at the back between mugs, cheese stacks before it.</summary>
        public static PropPlacement[] TeaTable() =>
            OnPlates(0f, 0.34f, 1, PropKind.Teapot, OnCentre)
                .Concat(new[] { Mug(-0.78f, 0.45f), Mug(0.78f, 0.45f) })
                .Concat(new[] { -0.8f, -0.45f, 0.45f, 0.8f }.SelectMany(x => Stack(PropKind.Cheese, 2, x, -0.15f))).ToArray();

        /// <summary>
        /// Cheese stacks on the back desk (light props only up there); on the main desk a teapot at the side outside its
        /// span, a plate of apples and cheese between them.
        /// </summary>
        public static PropPlacement[] CheeseOnTheBackDesk() =>
            new[] { -0.4f, 0f, 0.4f }.SelectMany(x => Stack(PropKind.Cheese, 2, x, 0f)).Select(OnBack)
                .Append(Food(PropKind.Teapot, -0.62f, 0.35f))
                .Concat(OnPlates(0.45f, 0.3f, 1, PropKind.Apple, FourOnPlate))
                .Append(Food(PropKind.Cheese, -0.5f, -0.15f)).ToArray();

        /// <summary>Feast: afternoon tea, the teapot on its plate at the back, plates of apples, cheese stacked all round.</summary>
        public static PropPlacement[] AfternoonTea() =>
            OnPlates(FrontLeft.x, FrontLeft.y, 1, PropKind.Apple, FiveOnPlate)
                .Concat(OnPlates(FrontRight.x, FrontRight.y, 1, PropKind.Apple, FiveOnPlate))
                .Concat(OnPlates(BackCentre.x, BackCentre.y, 1, PropKind.Teapot, OnCentre))
                .Concat(Lane.SelectMany(z => Stack(PropKind.Cheese, 2, 0f, z).Select(Sideways)))
                .Concat(CornerStacks.SelectMany(c => Stack(PropKind.Cheese, 3, c.x, c.y)))
                .Concat(FrontCorners.Select(c => Food(PropKind.Apple, c.x, c.y))).ToArray();

        /// <summary>Hard: a wide spread, few shots. Bowls of tomatoes at the back, tomatoes and cheese in rows.</summary>
        public static PropPlacement[] WideSpread() =>
            new[] { -0.65f, 0f, 0.65f }.SelectMany(x => InBowl(x, 0.45f, 1))
                .Concat(Line(PropKind.Tomato, 0f, -0.8f, -0.4f, 0f, 0.4f, 0.8f))
                .Concat(Line(PropKind.Cheese, -0.45f, -0.8f, -0.4f, 0f, 0.4f, 0.8f)).ToArray();

        /// <summary>New: the jam jar and the cupcake. A plate of cupcakes, cupcake pairs at the sides, jars in front.</summary>
        public static PropPlacement[] CupcakesAndJam() =>
            OnPlates(0f, 0.3f, 1, PropKind.Cupcake, FiveOnPlate)
                .Concat(new[] { -0.75f, 0.75f }.SelectMany(x => Line(PropKind.Cupcake, 0.25f, x).Concat(Line(PropKind.Cupcake, 0.58f, x))))
                .Concat(Line(PropKind.JamJar, -0.45f, -0.75f, -0.38f, 0f, 0.38f, 0.75f)).ToArray();

        /// <summary>Feast: the bake sale, cupcakes on every plate and on every jam jar, down the lane and in the corners.</summary>
        public static PropPlacement[] BakeSale() =>
            ThreePlates(PropKind.Cupcake, FiveOnPlate, PropKind.Cupcake, FiveOnPlate, PropKind.Cupcake, FiveOnPlate)
                .Concat(Lane.Select(z => new Vector2(0f, z)).Concat(Corners)
                    .SelectMany(c => new[] { Food(PropKind.JamJar, c.x, c.y), Food(PropKind.Cupcake, c.x, c.y, JamJar) })).ToArray();

        // ---- helpers shared with Room2LayoutsLater ----

        private static PropPlacement Food(PropKind kind, float x, float z, float y = 0f, float yaw = 0f) => Prop(kind, x, y, z, 0, yaw);

        /// <summary>The feast frame's three plates, each with its food.</summary>
        private static IEnumerable<PropPlacement> ThreePlates(PropKind left, Vector2[] leftSpots, PropKind right, Vector2[] rightSpots,
            PropKind back, Vector2[] backSpots, int backLayers = 1) =>
            OnPlates(FrontLeft.x, FrontLeft.y, 1, left, leftSpots)
                .Concat(OnPlates(FrontRight.x, FrontRight.y, 1, right, rightSpots))
                .Concat(OnPlates(BackCentre.x, BackCentre.y, 1, back, backSpots, backLayers));

        /// <summary>Turned a quarter: a cheese wedge then fits the 0.36-wide lane.</summary>
        private static PropPlacement Sideways(PropPlacement placement)
        {
            placement.yawDegrees = 90f;
            return placement;
        }

        private static PropPlacement Mug(float x, float z) => Prop(PropKind.Mug, x, 0f, z, 0, 90f); // handle sideways: 0.43 wide

        private static IEnumerable<PropPlacement> Line(PropKind kind, float z, params float[] xs) => xs.Select(x => Food(kind, x, z));

        /// <summary>A stack of one kind (box-shaped foods, plates, jars, cans), each resting on the one below.</summary>
        private static IEnumerable<PropPlacement> Stack(PropKind kind, int count, float x, float z, float y0 = 0f) =>
            Enumerable.Range(0, count).Select(i => Food(kind, x, z, y0 + i * Height(kind)));

        /// <summary>plates dinner plates stacked at (x, z), food at the spots on the top one, layers deep.</summary>
        private static IEnumerable<PropPlacement> OnPlates(float x, float z, int plates, PropKind food, Vector2[] spots, int layers = 1)
        {
            foreach (var plate in Stack(PropKind.DinnerPlate, plates, x, z)) yield return plate;
            float top = plates * DinnerPlate;
            foreach (var spot in spots)
                foreach (var item in Stack(food, layers, x + spot.x, z + spot.y, top)) yield return item;
        }

        /// <summary>A bowl with tomatoes stacked on its rim.</summary>
        private static IEnumerable<PropPlacement> InBowl(float x, float z, int tomatoes) =>
            Stack(PropKind.Bowl, 1, x, z).Concat(Stack(PropKind.Tomato, tomatoes, x, z, Bowl));

        /// <summary>count dishes, plate and bowl in turn from a plate up.</summary>
        private static IEnumerable<PropPlacement> DishTower(float x, float z, int count)
        {
            float y = 0f;
            for (int i = 0; i < count; i++)
            {
                var kind = i % 2 == 0 ? PropKind.DinnerPlate : PropKind.Bowl;
                yield return Food(kind, x, z, y);
                y += Height(kind);
            }
        }

        private static float DishTowerHeight(int count) =>
            Enumerable.Range(0, count).Sum(i => i % 2 == 0 ? DinnerPlate : Bowl);

        private static float Height(PropKind kind) => kind switch
        {
            PropKind.Mug => LayoutKit.Mug,
            PropKind.PaperCup => Cup,
            PropKind.DinnerPlate => DinnerPlate,
            PropKind.Bowl => Bowl,
            PropKind.Teapot => Teapot,
            PropKind.WineGlass => WineGlass,
            PropKind.JamJar => JamJar,
            PropKind.KetchupBottle => KetchupBottle,
            PropKind.DrinkCan => DrinkCan,
            PropKind.Apple => Apple,
            PropKind.Tomato => Tomato,
            PropKind.Cheese => Cheese,
            PropKind.Burger => Burger,
            PropKind.Cupcake => Cupcake,
            PropKind.Croissant => Croissant,
            PropKind.Sandwich => Sandwich,
            _ => throw new System.ArgumentException($"no Kitchen height for {kind}"),
        };
    }
}
