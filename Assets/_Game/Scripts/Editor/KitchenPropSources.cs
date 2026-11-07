using System.Collections.Generic;
using Game.Core;
using Game.Levels;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// The Kitchen's kinds (Kitchen spec §5): dishes and glass shatter, food splashes, the drink can pops. Each is true
    /// to size against the mug (0.35 tall, a real 10 cm mug, so 1 real cm is 0.035): the scales below take each pack
    /// model to its real-world size. Masses follow the material; tune on device.
    /// </summary>
    internal static class KitchenPropSources
    {
        private const string TinyTreats = "TinyTreats";
        private const string Restaurant = "KayKitRestaurant";
        private const string BakedGoods = "TinyTreatsBakedGoods";
        private const string Art = PropPrefabBuilder.Art;
        private const float DinnerPlateScale = 1.067f;  // 0.75 wide → 0.80 (a 23 cm plate)
        private const float BowlScale = 1.25f;          // 0.40 wide → 0.50 (15 cm)
        private const float TeapotScale = 0.814f;       // 0.61 tall → 0.50 (15 cm)
        private const float WineGlassScale = 1.11f;     // 0.45 tall → 0.50 (14 cm): a wide-bowled model, 18 cm looked oversized
        private const float JamJarScale = 0.726f;       // 0.48 tall → 0.35 (10 cm)
        private const float KetchupScale = 0.9f;        // 0.78 tall → 0.70 (20 cm)
        private const float DrinkCanScale = 0.83f;      // 0.51 tall → 0.42 (12 cm)
        private const float AppleScale = 0.615f;        // 0.44 across → 0.27 (7.5 cm)
        private const float TomatoScale = 0.333f;       // 0.75 across → 0.25 (7 cm)
        private const float CheeseScale = 0.933f;       // 0.375 long → 0.35 (10 cm wedge)
        private const float BurgerScale = 0.425f;       // 0.82 wide → 0.35 (10 cm)
        private const float CupcakeScale = 0.49f;       // 0.51 tall → 0.25 (7 cm)
        private const float CroissantScale = 0.57f;     // 0.74 long → 0.42 (12 cm)
        private const float SandwichScale = 0.56f;      // 0.62 wide → 0.35 (10 cm)

        internal static IEnumerable<PropPrefabBuilder.Source> All()
        {
            var china = PropPrefabBuilder.WritePhysicsMaterial("China", 0.25f, 0.3f, 0.05f);
            var glass = PropPrefabBuilder.WritePhysicsMaterial("Glass", 0.3f, 0.4f, 0.05f);
            var food = PropPrefabBuilder.WritePhysicsMaterial("Food", 0.45f, 0.55f, 0.02f);
            yield return Dish(PropKind.DinnerPlate, $"Packs/{TinyTreats}/plate_A.fbx", 0.2f, china, DinnerPlateScale);
            yield return Dish(PropKind.Bowl, $"Packs/{TinyTreats}/bowl.fbx", 0.25f, china, BowlScale);
            yield return Dish(PropKind.Teapot, $"Packs/{TinyTreats}/teapot.fbx", 0.6f, china, TeapotScale);
            yield return Dish(PropKind.WineGlass, $"Packs/{TinyTreats}/wine_glass.fbx", 0.15f, glass, WineGlassScale);
            // Glass and plastic, but what shows is the jam and the ketchup, so they splash (Kitchen plan, Claude's call).
            yield return Food(PropKind.JamJar, TinyTreats, $"Packs/{TinyTreats}/jam.fbx", 0.3f, glass, JamJarScale);
            yield return Food(PropKind.KetchupBottle, Restaurant, $"Packs/{Restaurant}/ketchup.fbx", 0.35f, glass, KetchupScale);
            yield return new PropPrefabBuilder.Source
            {
                kind = PropKind.DrinkCan, atlas = TinyTreats, collider = PropColliderShape.ConvexMesh, mass = 0.15f,
                physics = PropPrefabBuilder.WritePhysicsMaterial("DrinkCan", 0.4f, 0.5f, 0.15f), breakMode = BreakMode.Pop,
                variants = new[] { Art + $"Packs/{TinyTreats}/drink_can.fbx" }, visualScale = DrinkCanScale,
            };
            yield return Food(PropKind.Apple, TinyTreats, $"Packs/{TinyTreats}/apple.fbx", 0.12f, food, AppleScale);
            // Round or flat foods that must settle get boxes, as the donut did: the tomato's hull rocks on its round
            // bottom, and a hull's rounded top never lets anything stack on it.
            // The owner's soft foods splash the moment they are touched, not only on the floor.
            yield return Soft(Food(PropKind.Tomato, Restaurant, $"Packs/{Restaurant}/food_ingredient_tomato.fbx", 0.1f, food, TomatoScale, PropColliderShape.Box));
            yield return Soft(Food(PropKind.Cheese, TinyTreats, $"Packs/{TinyTreats}/cheese_A.fbx", 0.15f, food, CheeseScale, PropColliderShape.Box));
            yield return Food(PropKind.Burger, Restaurant, "Generated/Burger.fbx", 0.15f, food, BurgerScale);
            yield return Soft(Food(PropKind.Cupcake, BakedGoods, $"Packs/{BakedGoods}/cupcake.fbx", 0.06f, food, CupcakeScale));
            yield return Food(PropKind.Croissant, BakedGoods, $"Packs/{BakedGoods}/croissant.fbx", 0.05f, food, CroissantScale);
            yield return Food(PropKind.Sandwich, TinyTreats, $"Packs/{TinyTreats}/sandwich.fbx", 0.1f, food, SandwichScale, PropColliderShape.Box);
        }

        private static PropPrefabBuilder.Source Dish(PropKind kind, string model, float mass, PhysicsMaterial physics, float scale) =>
            Breakable(kind, TinyTreats, model, mass, physics, scale, BreakMode.Shatter, PropColliderShape.ConvexMesh);

        private static PropPrefabBuilder.Source Food(PropKind kind, string atlas, string model, float mass, PhysicsMaterial physics,
            float scale, PropColliderShape collider = PropColliderShape.ConvexMesh) =>
            Breakable(kind, atlas, model, mass, physics, scale, BreakMode.Splash, collider);

        private static PropPrefabBuilder.Source Soft(PropPrefabBuilder.Source food)
        {
            food.splashesOnTouch = true;
            return food;
        }

        private static PropPrefabBuilder.Source Breakable(PropKind kind, string atlas, string model, float mass,
            PhysicsMaterial physics, float scale, BreakMode mode, PropColliderShape collider) => new PropPrefabBuilder.Source
        {
            kind = kind, atlas = atlas, collider = collider, mass = mass, physics = physics, breakMode = mode,
            // Only glass and china break into pieces; food squashes whole (Squash).
            variants = new[] { Art + model }, visualScale = scale,
            broken = mode == BreakMode.Shatter ? new[] { Art + $"Generated/{kind}_broken.fbx" } : null,
        };
    }
}
