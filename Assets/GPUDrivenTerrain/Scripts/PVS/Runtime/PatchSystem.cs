
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PVS
{
    public class PatchSystem
    {
        private PatchAsset m_patchAsset;
        //临时用unity terrain获取高度，正式项目要用Height bytes
        private Terrain m_terrain;
        //正式项目需要做流式加载
        public void SetPatchData(PatchAsset patchAsset)
        {
            m_patchAsset = patchAsset;
        }

        public void SetTerrain(Terrain terrain)
        {
            m_terrain = terrain;
        }
        public List<SinglePatch> GetPatchDataByCameraPos(Vector3 worldPos)
        {
            Vector3Int logicPos = PatchUtility.WorldToLogicPos(m_terrain, worldPos, false, m_patchAsset.mapMinMaxPos);
            if (!m_patchAsset.allPosPatchDict.ContainsKey(logicPos))
            {
                return null;
            }
            var allPatch = m_patchAsset.allPosPatchDict[logicPos];
            return allPatch;
        }
        
        private static PatchSystem m_instance;
        public static PatchSystem Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new PatchSystem();
                }

                return m_instance;
            }
            private set { }
        }
  
    }
}

