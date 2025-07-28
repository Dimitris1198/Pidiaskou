using System;
using UnityEngine;
using System.Collections.Generic;


[Serializable]
public class PositionsDTO
{
    public List<Vector3> positions;

    public PositionsDTO(List<Vector3> data)
    {
        positions = data;
    }
}