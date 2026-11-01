import unittest

from lib import colors


PALETTE = {(0, 0): (1.0, 0.5, 0.0), (1, 0): (0.2, 0.4, 0.9), (2, 0): (0.95, 0.9, 0.8)}


class ColorTests(unittest.TestCase):
    def test_exact_color_maps_to_its_cell(self):
        self.assertEqual(colors.nearest_cell((0.2, 0.4, 0.9), PALETTE), (1, 0))

    def test_near_color_maps_to_nearest(self):
        self.assertEqual(colors.nearest_cell((0.97, 0.55, 0.1), PALETTE), (0, 0))
        self.assertEqual(colors.nearest_cell((0.9, 0.88, 0.82), PALETTE), (2, 0))

    def test_lightness_matters_less_than_hue_for_tints(self):
        # A darker orange is still orange, not the cream cell.
        self.assertEqual(colors.nearest_cell((0.6, 0.3, 0.0), PALETTE), (0, 0))


if __name__ == "__main__":
    unittest.main()
