"""Renders the cat model beside a pack mug and keycaps (front and three-quarter views) for style reference.

Usage: Blender --background --python render_refs.py -- <out_dir> [cat.fbx]
"""
import math
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))

import bpy  # noqa: E402

from lib import palette  # noqa: E402

ROOT = os.path.normpath(os.path.join(HERE, "..", "..", ".."))
ART = os.path.join(ROOT, "Assets", "_Game", "Art")


def material(png):
    mat = bpy.data.materials.new(os.path.basename(png))
    tree = mat.node_tree
    bsdf = tree.nodes["Principled BSDF"]
    tex = tree.nodes.new("ShaderNodeTexImage")
    tex.image = bpy.data.images.load(png)
    tex.interpolation = 'Closest'
    tree.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
    bsdf.inputs["Roughness"].default_value = 0.5
    return mat


def place(path, location, mat, yaw=0.0):
    before = set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=path)
    new = [o for o in bpy.data.objects if o not in before]
    for o in new:
        if o.type == 'MESH':
            o.data.materials.clear()
            o.data.materials.append(mat)
        if o.parent is None:
            o.location = location
            o.rotation_euler.z += yaw


def render(out, cam_location, cam_rotation):
    cam = bpy.data.objects["Cam"]
    cam.location = cam_location
    cam.rotation_euler = [math.radians(a) for a in cam_rotation]
    bpy.context.scene.render.filepath = out
    bpy.ops.render.render(write_still=True)


def main():
    argv = sys.argv[sys.argv.index("--") + 1:]
    out_dir = argv[0]
    cat = argv[1] if len(argv) > 1 else os.path.join(ART, "Generated", "Cat.fbx")
    os.makedirs(out_dir, exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    tiny = material(palette.ATLAS_PATH)
    place(cat, (0, 0, 0), tiny)
    place(os.path.join(ART, "Packs", "TinyTreats", "mug_A_pink.fbx"), (0.55, 0.1, 0), tiny)
    place(os.path.join(ART, "Generated", "Keycap1u_cream.fbx"), (-0.45, 0, 0), tiny)
    place(os.path.join(ART, "Generated", "Keycap1u_sky.fbx"), (-0.45, 0, 0.11), tiny)
    cam = bpy.data.objects.new("Cam", bpy.data.cameras.new("Cam"))
    bpy.context.scene.collection.objects.link(cam)
    bpy.context.scene.camera = cam
    sun = bpy.data.objects.new("Sun", bpy.data.lights.new("Sun", 'SUN'))
    bpy.context.scene.collection.objects.link(sun)
    sun.rotation_euler = (math.radians(50), math.radians(-20), math.radians(-30))
    sun.data.energy = 3.5
    world = bpy.data.worlds.new("W")
    world.color = (0.95, 0.88, 0.85)
    bpy.context.scene.world = world
    scene = bpy.context.scene
    scene.render.engine = 'BLENDER_EEVEE'
    scene.render.resolution_x = 1024
    scene.render.resolution_y = 1024
    render(os.path.join(out_dir, "cat_front.png"), (0, -2.2, 0.55), (82, 0, 0))
    render(os.path.join(out_dir, "cat_three_quarter.png"), (1.5, -1.6, 0.8), (75, 0, 43))


if __name__ == "__main__":
    main()
