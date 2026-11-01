"""Armature from a landmarks JSON plus rigid skinning: every part follows exactly one bone (chunky toy-like motion)."""
import json

import bpy


def armature(landmarks_path, name="CatRig"):
    with open(landmarks_path) as f:
        bones = json.load(f)["bones"]
    data = bpy.data.armatures.new(name)
    rig = bpy.data.objects.new(name, data)
    bpy.context.scene.collection.objects.link(rig)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.object.mode_set(mode='EDIT')
    edit = {}
    for bone_name, spec in bones.items():
        bone = data.edit_bones.new(bone_name)
        bone.head = spec["head"]
        bone.tail = spec["tail"]
        edit[bone_name] = bone
    for bone_name, spec in bones.items():
        if spec["parent"]:
            edit[bone_name].parent = edit[spec["parent"]]
    bpy.ops.object.mode_set(mode='OBJECT')
    return rig


def skin_rigid(rig, parts_by_bone, name="Cat"):
    """Joins all parts into one mesh, each part's vertices weighted 1.0 to its bone, parented to the rig."""
    groups = []
    for bone_name, parts in parts_by_bone.items():
        for part in parts:
            group = part.vertex_groups.new(name=bone_name)
            group.add(range(len(part.data.vertices)), 1.0, 'REPLACE')
            groups.append(part)
    bpy.ops.object.select_all(action='DESELECT')
    for part in groups:
        part.select_set(True)
    bpy.context.view_layer.objects.active = groups[0]
    bpy.ops.object.join()
    mesh = bpy.context.view_layer.objects.active
    mesh.name = name
    mesh.data.name = name
    mesh.parent = rig
    modifier = mesh.modifiers.new("Armature", 'ARMATURE')
    modifier.object = rig
    return mesh
