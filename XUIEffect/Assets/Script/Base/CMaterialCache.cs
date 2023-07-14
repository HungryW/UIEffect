
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIEffect
{
    public class CMaterialCache
    {
        private class CMaterialEntry
        {
            public Material m_mat;
            public int m_nRefCount;

            public void Release()
            {
                if (m_mat != null)
                {
                    UnityEngine.Object.DestroyImmediate(m_mat, false);
                }
                m_mat = null;
            }
        }

        private static Dictionary<Hash128, CMaterialEntry> ms_mapMat = new Dictionary<Hash128, CMaterialEntry>();


        public static Material Register(Material a_baseMat, Hash128 a_hash
                                        , Action<Material, Graphic> a_fnModifyMat, Graphic a_graphic)
        {
            if (!a_hash.isValid)
            {
                return null;
            }
            CMaterialEntry entry;
            if (!ms_mapMat.TryGetValue(a_hash, out entry))
            {
                entry = new CMaterialEntry();
                entry.m_mat = new Material(a_baseMat);
                entry.m_mat.hideFlags = HideFlags.HideAndDontSave;
                a_fnModifyMat(entry.m_mat, a_graphic);
                ms_mapMat.Add(a_hash, entry);
            }
            entry.m_nRefCount++;
            return entry.m_mat;
        }

        public static void UnRegister(Hash128 a_hash)
        {
            CMaterialEntry entry;
            if (!a_hash.isValid || !ms_mapMat.TryGetValue(a_hash, out entry))
            {
                return;
            }
            entry.m_nRefCount--;
            if (entry.m_nRefCount > 0)
            {
                return;
            }
            ms_mapMat.Remove(a_hash);
            entry.Release();
        }

    }
}


