"""Builds Generated/Cat.fbx: blockout mesh + rig + five takes. Exported without the authored turn (a skinned mesh
must keep its bind pose); the scene sets the cat's facing by yaw."""
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))

import bpy  # noqa: E402

from cat import actions, blockout, rig  # noqa: E402
from lib import export  # noqa: E402


def build(out, landmarks=os.path.join(HERE, "landmarks_blockout.json")):
    export.reset()
    bpy.context.scene.render.fps = actions.FPS
    armature = rig.armature(landmarks)
    rig.skin_rigid(armature, blockout.build())
    actions.build(armature)
    export.fbx([armature], os.path.join(out, "Cat.fbx"), with_animation=True, authored=False)


if __name__ == "__main__":
    build(export.args().out)
