using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PeaksDTO
{
    public List<List<float>> peaks { set; get; }

    public PeaksDTO(List<List<float>> data)
    {
        peaks = data;
    }
}