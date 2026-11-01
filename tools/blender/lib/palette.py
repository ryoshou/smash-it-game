"""Named cells of the Tiny Treats 1.1 atlas (8 x 4 vertical gradients, light at the top of each cell).

(col, row) are UV cells: row 0 is the bottom image row. Names were read off the atlas png.
"""
import os

ATLAS_PATH = os.path.normpath(os.path.join(
    os.path.dirname(__file__), "..", "..", "..",
    "Assets", "_Game", "Art", "Packs", "TinyTreats", "tiny_treats_texture_1.png"))
ATLAS_MATERIAL = "Atlas_TinyTreats"
GRID = (8, 4)

CELLS = {
    # top image row (UV row 3)
    "charcoal": (0, 3), "slate": (1, 3), "lavender_gray": (2, 3), "white": (3, 3),
    "cream": (4, 3), "apricot": (5, 3), "terracotta": (6, 3), "tan": (7, 3),
    # UV row 2
    "grass": (0, 2), "pine": (1, 2), "teal": (2, 2), "sky": (3, 2),
    "blue": (4, 2), "cyan": (5, 2), "periwinkle": (6, 2), "brown": (7, 2),
    # UV row 1
    "orange": (0, 1), "yellow": (1, 1), "red": (2, 1), "pink": (3, 1),
    "peach": (4, 1), "rose": (5, 1), "sage": (6, 1), "silver": (7, 1),
    # bottom image row (UV row 0)
    "butter": (0, 0), "ocean": (1, 0), "aqua": (2, 0), "gray": (3, 0),
    "gray_mid": (4, 0), "gray_dark": (5, 0), "glass": (6, 0), "glass_light": (7, 0),
}
