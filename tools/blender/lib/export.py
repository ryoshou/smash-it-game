"""FBX export with Unity-friendly axes (Y up, Blender -Y faces the player = Unity -Z) and one atlas material slot."""
import argparse
import os
import sys

import bpy

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


def fbx(objects, path, with_animation=False, material=ATLAS_MATERIAL):
    """Exports `objects` (and their children) to `path`, transforms applied, scale 1."""
    os.makedirs(os.path.dirname(path), exist_ok=True)
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
