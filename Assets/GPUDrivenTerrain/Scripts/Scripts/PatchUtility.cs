using System.Collections;
using System.Collections.Generic;
using PVS;
using UnityEngine;

public static class PatchUtility
{
    /// <summary>
    /// logicPos: {[0,0,0],[63,63,2]}
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public static Vector3Int WorldToLogicPos(Terrain terrain, Vector3 worldPos)
    {
        Vector3Int logicPos = new Vector3Int();
        int sampleTileSize = CameraSample.SampleTileSize;
        logicPos.x = ((int)worldPos.x) / sampleTileSize;
        logicPos.z = ((int)worldPos.z) / sampleTileSize;
        Vector3 pos = new Vector3(logicPos.x * sampleTileSize + sampleTileSize / 2, 0,
            logicPos.z * sampleTileSize + sampleTileSize / 2);
        float terrainHeight = terrain.SampleHeight(pos);
        float initHeight = terrainHeight + CameraSample.SampleInitHeight;
        logicPos.y = Mathf.RoundToInt((worldPos.y - initHeight) / CameraSample.SampleHeightStep);
        return logicPos;
    }
}
