using UnityEngine;

namespace Game.Editor
{
    /// <summary>A candy colour: the face's top colour (the sprite shades it darker downwards) and the rim, lip and text stroke.</summary>
    public readonly struct CandyLook
    {
        public static readonly CandyLook Green = new CandyLook(0x7BE35A, 0x1F6B14);
        public static readonly CandyLook Blue = new CandyLook(0x5FB8FF, 0x17457E);
        public static readonly CandyLook Red = new CandyLook(0xFF6B6B, 0x7A1414);
        public static readonly CandyLook Yellow = new CandyLook(0xFFD84A, 0x8A4B00);
        public static readonly CandyLook Purple = new CandyLook(0x5B4BD6, 0x241A6E);
        public static readonly CandyLook Orange = new CandyLook(0xFF9A3C, 0x7A2E00);
        public static readonly CandyLook Cream = new CandyLook(0xFFF6E6, 0x8A4B00);

        public readonly Color Top;
        public readonly Color Rim;

        private CandyLook(int top, int rim)
        {
            Top = Hex(top);
            Rim = Hex(rim);
        }

        public static Color Hex(int rgb) => new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);
    }
}
