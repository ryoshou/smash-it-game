"""FBX export with Unity-friendly axes and one atlas material slot.

Authored models use Blender +Y = away from the player = Unity +Z, and Blender +X = Unity +X. The FBX/Unity axis
conversion alone maps Blender (x, y, z) to Unity (-x, z, -y), so authored exports get a 180 degree turn about the
up axis baked into mesh data and empty positions (every node keeps an identity rotation, so builders that set a
model's rotation never undo it). Pack-derived exports (re-exported pack models, e.g. fracture sets) skip it: their
round trip is already identity with the original pack file.
"""
import argparse
import math
import os
import sys

import bpy
from mathutils import Matrix

from lib.palette import ATLAS_MATERIAL


def args():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--out", required=True)
    return parser.parse_args(argv)


def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def atlas_material(name=ATLAS_MATERIAL):
    return bpy.data.materials.get(name) or bpy.data.materials.new(name)


def assign_atlas(obj, name=ATLAS_MATERIAL):
    obj.data.materials.clear()
    obj.data.materials.append(atlas_material(name))


def _bake_turn(objects):
    """Turns the hierarchy 180 degrees about world Z, baked: meshes keep the turned world shape in their data and
    sit at identity; empties keep their turned world position with no rotation."""
    bpy.context.view_layer.update()  # generators set locations without updating; matrix_world must be current
    turn = Matrix.Rotation(math.pi, 4, 'Z')
    nodes = []
    for top in objects:
        nodes.extend([top, *top.children_recursive])
    worlds = {o.name: o.matrix_world.copy() for o in nodes}
    for o in nodes:
        if o.type == 'MESH':
            o.data.transform(turn @ worlds[o.name])
    for o in nodes:  # parents come before their children in this order
        if o.type == 'MESH':
            o.matrix_world = Matrix.Identity(4)
        else:
            o.matrix_world = Matrix.Translation(turn @ worlds[o.name].translation)
        bpy.context.view_layer.update()


def fbx(objects, path, with_animation=False, material=ATLAS_MATERIAL, authored=True):
    """Exports `objects` (and their children) to `path`, transforms applied, scale 1."""
    os.makedirs(os.path.dirname(path), exist_ok=True)
    if authored:
        _bake_turn(objects)
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:
        for o in [obj, *obj.children_recursive]:
            o.select_set(True)
            if o.type == 'MESH':
                assign_atlas(o, material)
    bpy.ops.export_scene.fbx(
        filepath=path, use_selection=True, object_types={'MESH', 'EMPTY', 'ARMATURE'},
        apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL', bake_space_transform=True,
        axis_forward='-Z', axis_up='Y', mesh_smooth_type='FACE', use_mesh_modifiers=True,
        add_leaf_bones=False, bake_anim=with_animation, bake_anim_use_all_actions=with_animation,
        bake_anim_use_nla_strips=False, bake_anim_force_startend_keying=with_animation)
    print(f"wrote {path}")
