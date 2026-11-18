# PSK/PSKX -> FBX for L2 effect static meshes.
# Rotation X -90, origin to geometry, world origin, FBX unit scale.
# Addon: io_import_scene_unreal_psa_psk_280.py (pskimport called directly).

import bpy
import math
import os
import sys
import importlib.util


def _argv_after_dash():
    if "--" not in sys.argv:
        raise SystemExit("usage: blender -b -P psk_to_fbx.py -- <psk> <fbx> <addon.py>")
    return sys.argv[sys.argv.index("--") + 1 :]


def _load_addon(path):
    spec = importlib.util.spec_from_file_location("io_import_scene_unreal_psa_psk_280", path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


def main():
    args = _argv_after_dash()
    if len(args) < 3:
        raise SystemExit("need <psk> <fbx> <addon.py>")
    psk_path, fbx_path, addon_path = args[0], args[1], args[2]
    os.makedirs(os.path.dirname(fbx_path) or ".", exist_ok=True)

    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)

    mod = _load_addon(addon_path)
    ok = mod.pskimport(
        psk_path,
        context=bpy.context,
        bScaleDown=True,
        bDontInvertRoot=True,
        bReorientBones=False,
    )
    if ok is False:
        raise SystemExit("pskimport failed: " + psk_path)

    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if not meshes:
        raise SystemExit("no mesh after pskimport: " + psk_path)

    bpy.ops.object.select_all(action="DESELECT")
    for obj in meshes:
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        obj.rotation_euler[0] -= math.radians(90)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        bpy.ops.object.origin_set(type="ORIGIN_GEOMETRY", center="BOUNDS")
        bpy.context.scene.cursor.location = (0.0, 0.0, 0.0)
        obj.location = (0.0, 0.0, 0.0)

    bpy.ops.export_scene.fbx(
        filepath=fbx_path,
        use_selection=False,
        object_types={"ARMATURE", "MESH"},
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        primary_bone_axis="Y",
        secondary_bone_axis="X",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=False,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        path_mode="AUTO",
    )
    print("WROTE " + fbx_path)


if __name__ == "__main__":
    main()
