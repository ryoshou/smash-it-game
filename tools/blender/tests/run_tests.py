"""Runs every tools/blender/tests/test_*.py inside Blender; exits 1 on any failure."""
import os
import sys
import unittest

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))

suite = unittest.defaultTestLoader.discover(HERE, pattern="test_*.py", top_level_dir=os.path.dirname(HERE))
result = unittest.TextTestRunner(stream=sys.stdout, verbosity=2).run(suite)
print("OK" if result.wasSuccessful() else "FAIL")
sys.exit(0 if result.wasSuccessful() else 1)
