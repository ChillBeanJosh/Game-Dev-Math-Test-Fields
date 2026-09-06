using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class GraphManager : MonoBehaviour
{
    [Header("Vehicle Response Graphs: ")]
    [SerializeField] private List<VehicleResponseGraph> vehicleResponseGraphs = new List<VehicleResponseGraph>();

    [Header("Global Graph Settings: ")]
    [SerializeField] private float _globalWindowTimeX = 7f;
    [SerializeField] private float _globalMaxDisplacementY = 0.16f;
    [SerializeField] private float _globalMinDisplacementY = -0.16f;

    public float GlobalWindowTimeX
    {
        get => _globalWindowTimeX;
        set
        {
            _globalWindowTimeX = value;
            ApplySettings();
        }
    }

    public float GlobalMaxDisplacementY
    {
        get => _globalMaxDisplacementY;
        set
        {
            _globalMaxDisplacementY = value;
            ApplySettings();
        }
    }

    public float GlobalMinDisplacementY
    {
        get => _globalMinDisplacementY;
        set
        {
            _globalMinDisplacementY = value;
            ApplySettings();
        }
    }

    private void ApplySettings()
    {
        foreach (VehicleResponseGraph graph in vehicleResponseGraphs)
        {
            graph.ApplyGraphSettings(_globalWindowTimeX, _globalMaxDisplacementY, _globalMinDisplacementY);
        }
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        ApplySettings();
    }

#endif

}