"""The cat's five takes (spec §12.3): Idle loop, Aim pose ramp (sampled by pull), Release, Celebrate, Annoyed.

Rotations are pose-bone Euler degrees about each bone's own axes (bones point up/along their chain, so X bends
forwards/backwards and Z twists/sways). 24 fps.
"""
import math

import bpy

FPS = 24


def _key(rig, frame, pose):
    """pose: {bone: (rx, ry, rz)} in degrees, or {"root.loc": (x, y, z)}."""
    for bone_name, value in pose.items():
        if bone_name == "root.loc":
            pb = rig.pose.bones["root"]
            pb.location = value
            pb.keyframe_insert("location", frame=frame)
            continue
        pb = rig.pose.bones[bone_name]
        pb.rotation_mode = 'XYZ'
        pb.rotation_euler = [math.radians(v) for v in value]
        pb.keyframe_insert("rotation_euler", frame=frame)


def _reset(rig):
    for pb in rig.pose.bones:
        pb.rotation_mode = 'XYZ'
        pb.rotation_euler = (0, 0, 0)
        pb.location = (0, 0, 0)


ALL = ["root", "spine", "chest", "neck", "head", "ear.L", "ear.R", "tail.1", "tail.2", "tail.3", "tail.4",
       "arm.L", "forearm.L", "paw.L", "arm.R", "forearm.R", "paw.R"]
REST = {b: (0, 0, 0) for b in ALL}


def _action(rig, name, keys):
    action = bpy.data.actions.new(name)
    action.use_fake_user = True
    rig.animation_data_create()
    rig.animation_data.action = action
    _reset(rig)
    for frame, pose in keys:
        full = dict(REST)
        full.update(pose)
        full.setdefault("root.loc", (0, 0, 0))
        _key(rig, frame, full)
    return action


def build(rig):
    aim = {"spine": (-12, 0, 0), "chest": (-8, 0, 0), "head": (10, 0, 0), "arm.L": (-45, 0, 0),
           "arm.R": (-45, 0, 0), "forearm.L": (20, 0, 0), "forearm.R": (20, 0, 0), "tail.1": (25, 0, 0)}
    forward = {"spine": (8, 0, 0), "chest": (6, 0, 0), "arm.L": (30, 0, 0), "arm.R": (30, 0, 0), "tail.1": (-15, 0, 0)}
    smug = {"head": (-16, 0, 6), "neck": (-6, 0, 0), "ear.L": (-10, 0, 0), "ear.R": (-10, 0, 0),
            "tail.1": (-25, 0, 0), "tail.2": (-20, 0, 0), "tail.3": (-15, 0, 0), "tail.4": (-10, 0, 0)}
    flat = {"ear.L": (45, 0, -20), "ear.R": (45, 0, 20)}

    def sway(deg):
        return {"tail.1": (0, 0, deg), "tail.2": (0, 0, deg * 1.3), "tail.3": (0, 0, deg * 1.6), "tail.4": (0, 0, deg * 2)}

    actions = [
        _action(rig, "Idle", [
            (0, {"chest": (0, 0, 0), **sway(-8)}),
            (12, {"chest": (3, 0, 0), "head": (0, 0, 3), **sway(0)}),
            (24, {"chest": (0, 0, 0), **sway(8)}),
            (36, {"chest": (3, 0, 0), "head": (0, 0, -3), **sway(0)}),
            (48, {"chest": (0, 0, 0), **sway(-8)}),
        ]),
        _action(rig, "Aim", [(0, {}), (FPS, aim)]),
        _action(rig, "Release", [(0, aim), (3, forward), (7, {})]),
        _action(rig, "Celebrate", [
            (0, {}), (5, {**smug, "root.loc": (0, 0, 0.05)}), (10, {**smug, "root.loc": (0, 0, 0)}),
            (15, {**smug, "root.loc": (0, 0, 0.03)}), (20, {**smug, "root.loc": (0, 0, 0)}), (29, {}),
        ]),
        _action(rig, "Annoyed", [
            (0, {}), (3, {**flat, "head": (0, 0, 12), **sway(25)}), (7, {**flat, "head": (0, 0, -12), **sway(-25)}),
            (11, {**flat, "head": (0, 0, 10), **sway(20)}), (15, {**flat, "head": (0, 0, -8)}), (24, {}),
        ]),
    ]
    rig.animation_data.action = actions[0]
    return actions
