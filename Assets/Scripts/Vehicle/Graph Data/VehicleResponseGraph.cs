using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class VehicleResponseGraph : Graphic
{
    [Header("Graph Grid Settings")]
    public float windowTimeX = 7f;
    [Space]
    public float maxDisplacementY = 0.16f;
    public float minDisplacementY = -0.16f;
    [Space]
    public float lineThickness = 2f;

    [Header("Graph Line Settings")]
    public Color chassis_LineColor;
    public Color wheel1_LineColor;
    public Color wheel2_LineColor;
    public Color wheel3_LineColor;
    public Color wheel4_LineColor;

    private List<Vector2> chassis_DisplacementData = new List<Vector2>();
    private List<Vector2> wheel1_DisplacementData = new List<Vector2>();
    private List<Vector2> wheel2_DisplacementData = new List<Vector2>();
    private List<Vector2> wheel3_DisplacementData = new List<Vector2>();
    private List<Vector2> wheel4_DisplacementData = new List<Vector2>();
    private float startTime;


    private void Start()
    {
        base.Start();
        startTime = Time.time;
    }

    public void GraphQuarterCar(Vector3 chassisPosition, Vector3 wheelPosition)
    {
        //Ensure Data Is Within The Graph Window Time:
        float elpasedTime = Time.time - startTime;
        if (elpasedTime > windowTimeX) return;

        //Store The Displacement Data For The Chassis And Wheel:
        chassis_DisplacementData.Add(new Vector2(elpasedTime, chassisPosition.y));
        wheel1_DisplacementData.Add(new Vector2(elpasedTime, wheelPosition.y));

        //Refresh or Rebuilds The Graph Data:
        SetVerticesDirty();

        Debug.Log
        (
            $"Time: {elpasedTime:F2} | " +
            $"Chassis: {chassisPosition.y:F4} | " +
            $"Wheel: {wheelPosition.y:F4}"
        );
    }

    public void GraphHalfCar(Vector3 chassisPosition, Vector3 wheel1Position, Vector3 wheel2Position)
    {
        //Ensure Data Is Within The Graph Window Time:
        float elpasedTime = Time.time - startTime;
        if (elpasedTime > windowTimeX) return;

        //Store The Displacement Data For The Chassis And Wheels:
        chassis_DisplacementData.Add(new Vector2(elpasedTime, chassisPosition.y));
        wheel1_DisplacementData.Add(new Vector2(elpasedTime, wheel1Position.y));
        wheel2_DisplacementData.Add(new Vector2(elpasedTime, wheel2Position.y));

        //Refresh or Rebuilds The Graph Data:
        SetVerticesDirty();

        Debug.Log
        (
            $"Time: {elpasedTime:F2} | " +
            $"Chassis: {chassisPosition.y:F4} | " +
            $"Wheel 1: {wheel1Position.y:F4} | " +
            $"Wheel 2: {wheel2Position.y:F4}"
        );
    }

    public void GraphFullCar(Vector3 chassisPosition, Vector3 wheel1Position, Vector3 wheel2Position, Vector3 wheel3Position, Vector3 wheel4Position)
    {
        //Ensure Data Is Within The Graph Window Time:
        float elpasedTime = Time.time - startTime;
        if (elpasedTime > windowTimeX) return;

        //Store The Displacement Data For The Chassis And Wheels:
        chassis_DisplacementData.Add(new Vector2(elpasedTime, chassisPosition.y));
        wheel1_DisplacementData.Add(new Vector2(elpasedTime, wheel1Position.y));
        wheel2_DisplacementData.Add(new Vector2(elpasedTime, wheel2Position.y));
        wheel3_DisplacementData.Add(new Vector2(elpasedTime, wheel3Position.y));
        wheel4_DisplacementData.Add(new Vector2(elpasedTime, wheel4Position.y));

        //Refresh or Rebuilds The Graph Data:
        SetVerticesDirty();

        Debug.Log
        (
            $"Time: {elpasedTime:F2} | " +
            $"Chassis: {chassisPosition.y:F4} | " +
            $"Wheel 1: {wheel1Position.y:F4} | " +
            $"Wheel 2: {wheel2Position.y:F4} | " +
            $"Wheel 3: {wheel3Position.y:F4} | " +
            $"Wheel 4: {wheel4Position.y:F4}"
        );
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        //Clear The Previous Graph Data:
        //base.OnPopulateMesh(vh);
        vh.Clear();

        //Setup The Graph Rect And Dimensions:
        Rect rect = rectTransform.rect;
        float graphWidth = rect.width;
        float graphHeight = rect.height;

        //Draw The Graph Lines For Chassis And Wheel Displacement:
        DrawLine(vh, chassis_DisplacementData, graphWidth, graphHeight, chassis_LineColor);
        DrawLine(vh, wheel1_DisplacementData, graphWidth, graphHeight, wheel1_LineColor);
        DrawLine(vh, wheel2_DisplacementData, graphWidth, graphHeight, wheel2_LineColor);
        DrawLine(vh, wheel3_DisplacementData, graphWidth, graphHeight, wheel3_LineColor);
        DrawLine(vh, wheel4_DisplacementData, graphWidth, graphHeight, wheel4_LineColor);
    }

    private void DrawLine(VertexHelper vh, List<Vector2> dataPoints, float graphWidth, float graphHeight, Color lineColor)
    {
        //Ensure There Are Enough Data Points To Draw A Line:
        if (dataPoints.Count < 2) return;

        int startIndex = vh.currentVertCount;

        for (int i = 0; i < dataPoints.Count - 1; i++)
        {
            Vector2 currentDataPoint = dataPoints[i];
            Vector2 nextDataPoint = dataPoints[i + 1];

            float X1 = Mathf.Clamp01(currentDataPoint.x / windowTimeX) * graphWidth - (graphWidth / 2f);
            float Y1 = Mathf.InverseLerp(minDisplacementY, maxDisplacementY, currentDataPoint.y) * graphHeight - (graphHeight / 2f);

            float X2 = Mathf.Clamp01(nextDataPoint.x / windowTimeX) * graphWidth - (graphWidth / 2f);
            float Y2 = Mathf.InverseLerp(minDisplacementY, maxDisplacementY, nextDataPoint.y) * graphHeight - (graphHeight / 2f);

            Vector2 startPoint = new Vector2(X1, Y1);
            Vector2 endPoint = new Vector2(X2, Y2);

            Vector2 direction = (endPoint - startPoint).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * (lineThickness / 2f);

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = lineColor;

            vertex.position = startPoint + normal;
            vh.AddVert(vertex);

            vertex.position = startPoint - normal;
            vh.AddVert(vertex);

            vertex.position = endPoint + normal;
            vh.AddVert(vertex);

            vertex.position = endPoint - normal;
            vh.AddVert(vertex);

            int index = startIndex + i * 4;

            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index + 1, index + 3, index + 2);
        }
    }
}   
