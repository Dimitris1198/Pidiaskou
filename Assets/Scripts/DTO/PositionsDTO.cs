using System;
using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class PositionsDTO
{
    public int maxClusters;
    public List<Vector3> positions;

    public PositionsDTO(int clusters, List<Vector3> data)
    {
        maxClusters = clusters;
        positions = data;
    }
}