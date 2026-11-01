"""Colour matching onto atlas cells (Task 10: an AI mesh's texture colours become atlas cells)."""
import math

from lib.palette import GRID

LIGHTNESS_WEIGHT = 0.5  # cells are light-to-dark gradients, so hue/chroma decide the cell more than lightness


def _linear(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def lab(rgb):
    """sRGB (0..1) to CIELAB (D65)."""
    r, g, b = (_linear(c) for c in rgb)
    x = (0.4124 * r + 0.3576 * g + 0.1805 * b) / 0.95047
    y = 0.2126 * r + 0.7152 * g + 0.0722 * b
    z = (0.0193 * r + 0.1192 * g + 0.9505 * b) / 1.08883

    def f(t):
        return t ** (1 / 3) if t > 0.008856 else 7.787 * t + 16 / 116

    fx, fy, fz = f(x), f(y), f(z)
    return 116 * fy - 16, 500 * (fx - fy), 200 * (fy - fz)


def nearest_cell(rgb, palette):
    """palette: {cell: rgb}. Returns the cell whose colour is closest (weighted CIELAB distance)."""
    target = lab(rgb)

    def distance(cell):
        other = lab(palette[cell])
        return math.sqrt((LIGHTNESS_WEIGHT * (target[0] - other[0])) ** 2
                         + (target[1] - other[1]) ** 2 + (target[2] - other[2]) ** 2)

    return min(palette, key=distance)


def atlas_palette(image, grid=GRID, exclude=()):
    """{cell: rgb} sampled at the middle of each cell of a loaded bpy image (sRGB pixel values)."""
    width, height = image.size
    pixels = image.pixels[:]
    cols, rows = grid
    palette = {}
    for col in range(cols):
        for row in range(rows):
            if (col, row) in exclude:
                continue
            x = int((col + 0.5) / cols * width)
            y = int((row + 0.5) / rows * height)
            i = (y * width + x) * 4
            palette[(col, row)] = (pixels[i], pixels[i + 1], pixels[i + 2])
    return palette
