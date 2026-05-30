using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class PendulumTrail : Graphic
{
    //Holds The List Of All Trail Points For Each Pendulum System Inside The Data List:
    private Dictionary<int, Queue<Vector2>> _systemTrails = new Dictionary<int, Queue<Vector2>>();
    //Holds The List Of All Trail Colors For Each Pendulum System Inside The Data List:
    private Dictionary<int, Color> _systemColors = new Dictionary<int, Color>();

    [Header("Trail Settings")]
    public float lineThickness = 2f;
    public int maxPointsPerSystem { get; private set; } = 1000;
    [Space]
    public bool isTrailActive = true;

    [Header("Trail Head Settings: ")]
    public bool showTrailHead = true;
    [Space]
    public float trailHeadSize = 5f;
    public Color trailHeadColor = Color.white;
    [Range(4, 16)] public int trailHeadSegments = 8;

    public void SetupVertexBudget(int pendulumCount, bool bothTrailsActive)
    {
        if(!isTrailActive)
        {
            maxPointsPerSystem = 0;
            ClearAllTrails();
            return;
        }

        if (pendulumCount > 0)
        {
            // to stay performant. If only ONE trail type is active, we can use the full allocation!
            int pointsAllocationDivisor = bothTrailsActive ? 8 : 4;

            // Formula: 60,000 vertices total / (Divisor * Number of Pendulums)
            maxPointsPerSystem = Mathf.Max(10, Mathf.FloorToInt(60000f / (pointsAllocationDivisor * pendulumCount)));
            Debug.Log($"Vertex Budget Setup: {maxPointsPerSystem} points per system for {pendulumCount} pendulums with both trails active: {bothTrailsActive}");
        }
        ClearAllTrails();
    }

    public void AddPoint(int systemId, Vector2 point, Color systemColor)
    {
        if(!isTrailActive) return;

        if (!_systemTrails.ContainsKey(systemId))
        {
            _systemTrails[systemId] = new Queue<Vector2>();
            _systemColors[systemId] = systemColor;
        }
        Queue<Vector2> pointsQueue = _systemTrails[systemId];
        pointsQueue.Enqueue(point);

        while (pointsQueue.Count > maxPointsPerSystem)
        {
            pointsQueue.Dequeue();
        }

        //Called To Refresh The Trail Whenever A New Point Is Added.
        //This Will Trigger The OnPopulateMesh Method To Rebuild The Trail Mesh With The Updated Points.
        SetAllDirty();
    }

    public void ClearAllTrails()
    {
        _systemTrails.Clear();
        _systemColors.Clear();
        SetAllDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        foreach (var keyValuePair in _systemTrails)
        {
            int systemId = keyValuePair.Key;
            Queue<Vector2> pointsQueue = keyValuePair.Value;
            Color systemColor = _systemColors[systemId];

            if (pointsQueue.Count < 2) continue;

            Vector2[] pointsArray = pointsQueue.ToArray();
            for (int i = 0; i < pointsArray.Length - 1; i++)
            {
                CreateLineSegment(pointsArray[i], pointsArray[i + 1], systemColor, vh);
            }
            if (showTrailHead)
            {
                DrawHeadCircle(pointsArray[pointsArray.Length - 1], systemColor, vh);
            }
        }
    }

    private void CreateLineSegment(Vector2 start, Vector2 end, Color segmentColor, VertexHelper vh)
    {
        //Opposite Normalized Direction Vector For The Line Segment, Scaled By Half The Line Thickness To Create A Quad:
        Vector2 direction = (end - start).normalized;
        Vector2 normal = new Vector2(-direction.y, direction.x) * (lineThickness / 2f);

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = segmentColor;

        int index = vh.currentVertCount;

        // Construct 4 corners of a continuous segment quad
        vertex.position = start - normal;
        vh.AddVert(vertex);

        vertex.position = start + normal;
        vh.AddVert(vertex);

        vertex.position = end + normal;
        vh.AddVert(vertex);

        vertex.position = end - normal;
        vh.AddVert(vertex);

        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);
    }

    private void DrawHeadCircle(Vector2 center, Color headColor, VertexHelper vh)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = headColor;

        int centerIndex = vh.currentVertCount;

        //Add Center Point Of Circle:
        vertex.position = center;
        vh.AddVert(vertex);

        //Variables For Circle Generation:
        float radius = trailHeadSize / 2f;
        int CircleSmoothness = trailHeadSegments;

        //Define Positions For Circle Circumference Vertices:
        for (int i = 0; i <= CircleSmoothness; i++)
        {
            float angle = (i * 2f * Mathf.PI) / CircleSmoothness;
            vertex.position = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            vh.AddVert(vertex);
        }

        //Create Triangles To Form The Circle:
        for (int i = 1; i <= CircleSmoothness; i++)
        {
            int nextIndex = (i == CircleSmoothness) ? centerIndex + 1 : centerIndex + i + 1;
            vh.AddTriangle(centerIndex, centerIndex + i, nextIndex);
        }
    }
}
