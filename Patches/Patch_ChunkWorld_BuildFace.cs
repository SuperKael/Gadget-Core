using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(ChunkWorld))]
    [HarmonyPatch("BuildFace")]
    static class Patch_ChunkWorld_BuildFace
    {
        [HarmonyPrefix]
        public static bool Prefix(ChunkWorld __instance, byte brick, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, int id, int face)
        {
            if (TileRegistry.GetSingleton().HasEntry(id))
            {
                TileInfo tile = TileRegistry.GetSingleton().GetEntry(id);
                if (tile.Type == TileType.SOLID || tile.Type == TileType.WALL)
                {
                    verts.Add(corner);
                    verts.Add(corner + up);
                    verts.Add(corner + up + right);
                    verts.Add(corner + right);
                }
                else
                {
                    if (face == 4)
                    {
                        verts.Add(corner + new Vector3(0f, 0f, 0.5f));
                        verts.Add(corner + new Vector3(0f, 0f, 0.5f) + up);
                        verts.Add(corner + new Vector3(0f, 0f, 0.5f) + up + right);
                        verts.Add(corner + new Vector3(0f, 0f, 0.5f) + right);
                    }
                    else if (face == 0 || face == 1)
                    {
                        verts.Add(corner + new Vector3(0f, 0f, 0.5f));
                        verts.Add(corner + up + new Vector3(0f, 0f, 0.5f));
                        verts.Add(corner + up + right);
                        verts.Add(corner + right);
                    }
                    else if (face != 5)
                    {
                        verts.Add(corner + new Vector3(0f, 0f, 0.5f));
                        verts.Add(corner + up);
                        verts.Add(corner + up + right);
                        verts.Add(corner + right + new Vector3(0f, 0f, 0.5f));
                    }
                    else
                    {
                        verts.Add(corner);
                        verts.Add(corner + up);
                        verts.Add(corner + up + right);
                        verts.Add(corner + right);
                    }
                }
                if (tile.Sprite != null)
                {
                    uvs.Add(new Vector2(tile.Sprite.coords.x, tile.Sprite.coords.y));
                    uvs.Add(new Vector2(tile.Sprite.coords.x, tile.Sprite.coords.y + 1));
                    uvs.Add(new Vector2(tile.Sprite.coords.x + 1, tile.Sprite.coords.y + 1));
                    uvs.Add(new Vector2(tile.Sprite.coords.x + 1, tile.Sprite.coords.y));
                }
                else
                {
                    uvs.Add(new Vector2(GadgetCoreAPI.MissingTexSprite.coords.x, GadgetCoreAPI.MissingTexSprite.coords.y));
                    uvs.Add(new Vector2(GadgetCoreAPI.MissingTexSprite.coords.x, GadgetCoreAPI.MissingTexSprite.coords.y + 1));
                    uvs.Add(new Vector2(GadgetCoreAPI.MissingTexSprite.coords.x + 1, GadgetCoreAPI.MissingTexSprite.coords.y + 1));
                    uvs.Add(new Vector2(GadgetCoreAPI.MissingTexSprite.coords.x + 1, GadgetCoreAPI.MissingTexSprite.coords.y));
                }
                if (reversed)
                {
                    tris.Add(verts.Count);
                    tris.Add(verts.Count + 1);
                    tris.Add(verts.Count + 2);
                    tris.Add(verts.Count + 2);
                    tris.Add(verts.Count + 3);
                    tris.Add(verts.Count);
                }
                else
                {
                    tris.Add(verts.Count + 1);
                    tris.Add(verts.Count);
                    tris.Add(verts.Count + 2);
                    tris.Add(verts.Count + 3);
                    tris.Add(verts.Count + 2);
                    tris.Add(verts.Count);
                }
                return false;
            }
            if (id == 2000 || id == 2001 || id == 2002 || id == 2200 || id == 2300)
            {
                return true;
            }
            else
            {
                verts.Add(corner);
                verts.Add(corner + up);
                verts.Add(corner + up + right);
                verts.Add(corner + right);

                uvs.Add(new Vector2(GadgetCoreAPI.MissingTexSprite.coords.x, GadgetCoreAPI.MissingTexSprite.coords.y));
                uvs.Add(new Vector2(GadgetCoreAPI.MissingTexSprite.coords.x, GadgetCoreAPI.MissingTexSprite.coords.y + 1));
                uvs.Add(new Vector2(GadgetCoreAPI.MissingTexSprite.coords.x + 1, GadgetCoreAPI.MissingTexSprite.coords.y + 1));
                uvs.Add(new Vector2(GadgetCoreAPI.MissingTexSprite.coords.x + 1, GadgetCoreAPI.MissingTexSprite.coords.y));

                if (reversed)
                {
                    tris.Add(verts.Count);
                    tris.Add(verts.Count + 1);
                    tris.Add(verts.Count + 2);
                    tris.Add(verts.Count + 2);
                    tris.Add(verts.Count + 3);
                    tris.Add(verts.Count);
                }
                else
                {
                    tris.Add(verts.Count + 1);
                    tris.Add(verts.Count);
                    tris.Add(verts.Count + 2);
                    tris.Add(verts.Count + 3);
                    tris.Add(verts.Count + 2);
                    tris.Add(verts.Count);
                }
                return false;
            }
        }
    }
}