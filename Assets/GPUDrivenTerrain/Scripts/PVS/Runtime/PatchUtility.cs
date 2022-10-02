using System;
using System.Collections;
using System.Collections.Generic;
using GPUDrivenTerrainLearn;
using PVS;
using UnityEngine;

public static class PatchUtility
{
    /// <summary>
    /// logicPos: {[0,0,0],[63,63,2]}
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public static Vector3Int WorldToLogicPos(Vector3 worldPos, bool bBake, Vector2Int mapMinMaxPos)
    {
        Vector3Int logicPos = new Vector3Int();
        int sampleTileSize = CameraSample.SampleTileSize;
        logicPos.x = ((int)worldPos.x) / sampleTileSize;
        logicPos.z = ((int)worldPos.z) / sampleTileSize;
        Vector3 pos = new Vector3(logicPos.x * sampleTileSize + sampleTileSize / 2, 0,
            logicPos.z * sampleTileSize + sampleTileSize / 2);

        float terrainHeight = 0;
        TerrainHeight.GetHeightInterpolated(worldPos, ref terrainHeight);
        float initHeight = terrainHeight + CameraSample.SampleInitHeight;
        logicPos.y = Mathf.RoundToInt((worldPos.y - initHeight) / CameraSample.SampleHeightStep);
        if (!bBake)
        {
            logicPos.y = Math.Clamp(logicPos.y, 0, CameraSample.SampleHeightNum);
            logicPos.x = Math.Clamp(logicPos.x, mapMinMaxPos.x, mapMinMaxPos.y);
            logicPos.z = Math.Clamp(logicPos.z, mapMinMaxPos.x, mapMinMaxPos.y);
        }
        
        return logicPos;
    }

}
