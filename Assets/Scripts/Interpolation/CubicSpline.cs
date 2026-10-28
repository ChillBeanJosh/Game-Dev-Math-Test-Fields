using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SplineSegment
{
    //4 Unknowns:
    public float a, b, c, d;

    //Domain:
    public float xStart, xEnd;
}

public class CubicSpline : MonoBehaviour
{
    [Header("Spline Configuration")]

    [Header("Data Nodes: ")]
    public List<Vector2> nodes = new List<Vector2>();


    [Header("Spline Type: ")]
    public bool isCubicSpline = true;
    [Space]
    public bool isNaturalSpline = true;
    public float leftFirstDerivative = 0f;
    public float rightFirstDerivative = 0f;


    [Header("Computed Functions: ")]
    public List<SplineSegment> functions = new List<SplineSegment>();


    [Header("Visualization: ")]
    public bool drawGizmos = true;
    [Range(10, 100)] public int resolution = 50;
    [Space]
    public bool showTangents = false;
    public bool showAccelerations = false;
    [Space]
    public float vectorScale = 0.2f;
    [Space]
    public Color nodeColor;
    public Color splineColor;
    public Color velocityColor;
    public Color accelerationColor;

    private void OnValidate()
    {
        //Ensure At Least 2 Points Exist:
        if (nodes != null && nodes.Count >= 2)
        {
            //Sort Nodes In Ascending Order:
            nodes.Sort((a, b) => a.x.CompareTo(b.x));

            //Recalculate Spline:
            CalculateSpline();
        }
    }

    public void CalculateSpline()
    {
        if (isCubicSpline)
        {
            CubicSplineCalc();
        }
        else
        {
            QuadraticSplineCalc();
        }
    }

    private void QuadraticSplineCalc()
    {
        int n = nodes.Count;
        if (n < 2) return;

        functions.Clear();
        int numberOfFunctions = n - 1;
        int totalUnknowns = 3 * numberOfFunctions;

        //A MATRIX (Coefficient Matrix):
        float[,] variableMatrix = new float[totalUnknowns, totalUnknowns];

        //B VECTOR (Constant Matrix):
        float[] constantMatrix = new float[totalUnknowns];

        int currentRow = 0;

        //1. INTERPOLATION CONDITIONS
        for (int i = 0; i < numberOfFunctions; i++)
        {
            //Each Function Contains 3 Unknowns:
            int colBase = 3 * i;

            //Left Domain:
            float xLeft = nodes[i].x;
            float yLeft = nodes[i].y;

            //*(FILL ROW IN A MATRIX)*
            //  [(a_1) * x_left^2]
            variableMatrix[currentRow, colBase + 0] = Mathf.Pow(xLeft, 2f);
            //  [(b_1) * x_left]
            variableMatrix[currentRow, colBase + 1] = xLeft;
            //  [(c_1) * 1]
            variableMatrix[currentRow, colBase + 2] = 1f;

            //*(FILL ROW IN B MATRIX)*
            //  [y_left]
            constantMatrix[currentRow] = yLeft;
            currentRow++;

            //Right Domain:
            float xRight = nodes[i + 1].x;
            float yRight = nodes[i + 1].y;

            //*(FILL ROW IN A MATRIX)*
            //  [(a_1) * x_right^2]
            variableMatrix[currentRow, colBase + 0] = Mathf.Pow(xRight, 2f);
            //  [(b_1) * x_right]
            variableMatrix[currentRow, colBase + 1] = xRight;
            //  [y_right]
            variableMatrix[currentRow, colBase + 2] = 1f;

            //*(FILL ROW IN B MATRIX)*
            constantMatrix[currentRow] = yRight;
            currentRow++;
        }

        //2. FIRST DERIVATIVE CONTINUITY: P'_i(x) = P'_i+1(x) 
        for (int i = 0; i < numberOfFunctions - 1; i++)
        {
            float sharedX = nodes[i + 1].x;

            int leftCol = 3 * i;
            //  [2 * (a_1) * x]
            variableMatrix[currentRow, leftCol + 0] = 2f * sharedX;
            //  [1 * (b_1)]
            variableMatrix[currentRow, leftCol + 1] = 1f;
            //  [0 * (c_1)]
            variableMatrix[currentRow, leftCol + 2] = 0f;

            int rightCol = 3 * (i + 1);
            //  [2 * (a_2) * x]
            variableMatrix[currentRow, rightCol + 0] = -2f * sharedX;
            //  [1 * (b_2)]
            variableMatrix[currentRow, rightCol + 1] = -1f;
            //  [0 * (c_2)]
            variableMatrix[currentRow, rightCol + 2] = 0f;

            constantMatrix[currentRow] = 0f;
            currentRow++;
        }

        //3. BOUNDARY CONDITION / ANCHOR: Set first function's quadratic coefficient a_0 = 0
        variableMatrix[currentRow, 0] = 1f;
        constantMatrix[currentRow] = 0f;
        currentRow++;

        //4. SOLVE MATRIX SYSTEM USING GAUSSIAN ELIMINATION
        float[] solvedCoefficients = GaussElimination(variableMatrix, constantMatrix);
        if (solvedCoefficients == null)
        {
            Debug.LogError("Matrix resolution failed!");
            return;
        }

        //5. PLUG IN UNKNOWNS INTO THE FUNCTIONS
        for (int i = 0; i < numberOfFunctions; i++)
        {
            int colBase = 3 * i;
            SplineSegment function = new SplineSegment();
            function.xStart = nodes[i].x;
            function.xEnd = nodes[i + 1].x;

            function.a = 0f;
            function.b = solvedCoefficients[colBase + 0];
            function.c = solvedCoefficients[colBase + 1];
            function.d = solvedCoefficients[colBase + 2];

            functions.Add(function);
        }
    }

    public void CubicSplineCalc()
    {
        int n = nodes.Count;
        if (n < 2) return;

        functions.Clear();
        int numberOfFunctions = n - 1;
        int totalUnknowns = 4 * numberOfFunctions;

        //A MATRIX (Coefficient Matrix):
        float[,] variableMatrix = new float[totalUnknowns, totalUnknowns];

        //B VECTOR (Constant Matrix):
        float[] constantMatrix = new float[totalUnknowns];

        int currentRow = 0;

        //1. INTERPOLATION CONDITIONS
        for (int i = 0; i < numberOfFunctions; i++)
        {
            //Each Function Contains 4 Unknowns:
            int colBase = 4 * i;


            //Left Domain:
            float xLeft = nodes[i].x;
            float yLeft = nodes[i].y;

            //*(FILL ROW IN A MATRIX)*
            //  [(a_1) * x_left^3]
            variableMatrix[currentRow, colBase + 0] = Mathf.Pow(xLeft, 3f);       
            //  [(b_1) * x_left^2]
            variableMatrix[currentRow, colBase + 1] = Mathf.Pow(xLeft, 2f);
            //  [(c_1) * x_left]
            variableMatrix[currentRow, colBase + 2] = xLeft;
            //  [(d_1) * 1]
            variableMatrix[currentRow, colBase + 3] = 1f;

            //*(FILL ROW IN B MATRIX)*
            //  [y_left]
            constantMatrix[currentRow] = yLeft;
            currentRow++;


            //Right Domain:
            float xRight = nodes[i + 1].x;
            float yRight = nodes[i + 1].y;

            //*(FILL ROW IN A MATRIX)*
            //  [(a_1) * x_right^3]
            variableMatrix[currentRow, colBase + 0] = Mathf.Pow(xRight, 3f);
            //  [(b_1) * x_right^2]
            variableMatrix[currentRow, colBase + 1] = Mathf.Pow(xRight, 2f);
            //  [(c_1) * x_right]
            variableMatrix[currentRow, colBase + 2] = xRight;
            //  [(d_1) * 1]
            variableMatrix[currentRow, colBase + 3] = 1f;

            //*(FILL ROW IN B MATRIX)*
            //  [y_right]
            constantMatrix[currentRow] = yRight;
            currentRow++;
        }

        //2. FIRST DERIVATIVE CONTINUITY: P'_i(x) = P'_i+1(x) 
        for (int i = 0; i < numberOfFunctions - 1; i++)
        {
            float sharedX = nodes[i + 1].x;


            int leftCol = 4 * i;

            //  [3 * (a_1) * x^2]
            variableMatrix[currentRow, leftCol + 0] = 3f * Mathf.Pow(sharedX, 2f);
            //  [2 * (b_1) * x]
            variableMatrix[currentRow, leftCol + 1] = 2f * sharedX;
            //  [1 * (c_1)]
            variableMatrix[currentRow, leftCol + 2] = 1f;
            //  [0 * (d_1)]
            variableMatrix[currentRow, leftCol + 3] = 0f;


            int rightCol = 4 * (i + 1);

            //  [3 * (a_2) * x^2]
            variableMatrix[currentRow, rightCol + 0] = -3f * Mathf.Pow(sharedX, 2f);
            //  [2 * (b_2) * x]
            variableMatrix[currentRow, rightCol + 1] = -2f * sharedX;
            //  [1 * (c_2)]
            variableMatrix[currentRow, rightCol + 2] = -1f;
            //  [0 * (d_2)]
            variableMatrix[currentRow, rightCol + 3] = 0f;

            constantMatrix[currentRow] = 0f;
            currentRow++;
        }

        //3. SECOND DERIVATIVE CONTINUITY: P''_i(x) = P''_i+1(x) 
        for (int i = 0; i < numberOfFunctions - 1; i++)
        {
            float sharedX = nodes[i + 1].x;


            int leftCol = 4 * i;

            //  [6 * (a_1) * x]
            variableMatrix[currentRow, leftCol + 0] = 6f * sharedX;
            //  [2 * (b_1)]
            variableMatrix[currentRow, leftCol + 1] = 2f;
            //  [0 * (c_1)]
            variableMatrix[currentRow, leftCol + 2] = 0f;
            //  [0 * (d_1)]
            variableMatrix[currentRow, leftCol + 3] = 0f;


            int rightCol = 4 * (i + 1);

            //  [6 * (a_2) * x]
            variableMatrix[currentRow, rightCol + 0] = -6f * sharedX;
            //  [2 * (b_2)]
            variableMatrix[currentRow, rightCol + 1] = -2f;
            //  [0 * (c_2)]
            variableMatrix[currentRow, rightCol + 2] = 0f;
            //  [0 * (d_2)]
            variableMatrix[currentRow, rightCol + 3] = 0f;

            constantMatrix[currentRow] = 0f;
            currentRow++;
        }

        //4. BOUNDARY CONDITIONS (Natural vs Clamped)
        float leftX = nodes[0].x;
        float rightX = nodes[n - 1].x;

        if (isNaturalSpline)
        {
            //Left Natural: P_0''(leftX) = 0
            variableMatrix[currentRow, 0] = 6f * leftX;
            variableMatrix[currentRow, 1] = 2f;
            constantMatrix[currentRow] = 0f;
            currentRow++;


            int lastCol = 4 * (numberOfFunctions - 1);

            //Right Natural: P_n-1''(rightX) = 0
            variableMatrix[currentRow, lastCol + 0] = 6f * rightX;
            variableMatrix[currentRow, lastCol + 1] = 2f;
            constantMatrix[currentRow] = 0f;
            currentRow++;
        }
        else
        {
            //Left Clamped: P_0'(leftX) = leftFirstDerivative
            variableMatrix[currentRow, 0] = 3f * Mathf.Pow(leftX, 2f);
            variableMatrix[currentRow, 1] = 2f * leftX;
            variableMatrix[currentRow, 2] = 1f;
            constantMatrix[currentRow] = leftFirstDerivative;
            currentRow++;


            int lastCol = 4 * (numberOfFunctions - 1);

            //Right Clamped: P_n-1'(rightX) = rightFirstDerivative
            variableMatrix[currentRow, lastCol + 0] = 3f * Mathf.Pow(rightX, 2f);
            variableMatrix[currentRow, lastCol + 1] = 2f * rightX;
            variableMatrix[currentRow, lastCol + 2] = 1f;
            constantMatrix[currentRow] = rightFirstDerivative;
            currentRow++;
        }

        //5. SOLVE MATRIX SYSTEM USING GAUSSIAN ELIMINATION
        float[] solvedCoefficients = GaussElimination(variableMatrix, constantMatrix);
        if (solvedCoefficients == null)
        {
            Debug.LogError("Matrix resolution failed!");
            return;
        }

        //6. PLUG IN UNKNOWNS INTO THE FUNCTIONS
        for (int i = 0; i < numberOfFunctions; i++)
        {
            int colBase = 4 * i;
            SplineSegment function = new SplineSegment();
            function.xStart = nodes[i].x;
            function.xEnd = nodes[i + 1].x;

            function.a = solvedCoefficients[colBase + 0];
            function.b = solvedCoefficients[colBase + 1];
            function.c = solvedCoefficients[colBase + 2];
            function.d = solvedCoefficients[colBase + 3];

            functions.Add(function);
        }
    }

    /// <summary>
    /// Solves system of linear equations A * X = B using Gaussian elimination with partial pivoting.
    /// </summary>
    private float[] GaussElimination(float[,] A, float[] B)
    {
        int n = B.Length;
        float[] X = new float[n];

        for (int p = 0; p < n; p++)
        {
            // Find pivot row
            int maxRow = p;
            for (int i = p + 1; i < n; i++)
            {
                if (Mathf.Abs(A[i, p]) > Mathf.Abs(A[maxRow, p]))
                {
                    maxRow = i;
                }
            }

            // Swap rows in A
            for (int k = p; k < n; k++)
            {
                float temp = A[p, k];
                A[p, k] = A[maxRow, k];
                A[maxRow, k] = temp;
            }

            // Swap corresponding values in B
            float tempB = B[p];
            B[p] = B[maxRow];
            B[maxRow] = tempB;

            // Singular check
            if (Mathf.Abs(A[p, p]) < 1e-9f) return null;

            // Pivot within A and B
            for (int i = p + 1; i < n; i++)
            {
                float alpha = A[i, p] / A[p, p];
                B[i] -= alpha * B[p];
                for (int k = p; k < n; k++)
                {
                    A[i, k] -= alpha * A[p, k];
                }
            }
        }

        // Back substitution
        for (int i = n - 1; i >= 0; i--)
        {
            float sum = 0f;
            for (int k = i + 1; k < n; k++)
            {
                sum += A[i, k] * X[k];
            }
            X[i] = (B[i] - sum) / A[i, i];
        }

        return X;
    }

    public SplineSegment GetFunctionForX(float x)
    {
        if (functions.Count == 0) return default;

        for (int i = 0; i < functions.Count; i++)
        {
            if (x >= functions[i].xStart && x <= functions[i].xEnd)
                return functions[i];
        }

        return x < functions[0].xStart ? functions[0] : functions[functions.Count - 1];
    }

    public float Evaluate(float x)
    {
        //[P(x) = a*x^3 + b*x^2 + c*x + d]
        SplineSegment f = GetFunctionForX(x);
        return f.a * Mathf.Pow(x, 3f) + f.b * Mathf.Pow(x, 2f) + f.c * x + f.d;
    }

    public float EvaluateFirstDerivative(float x)
    {
        //[P'(x) = 3a*x^2 + 2b*x + c]
        SplineSegment f = GetFunctionForX(x);
        return 3f * f.a * x * x + 2f * f.b * x + f.c;
    }

    public float EvaluateSecondDerivative(float x)
    {
        //[P''(x) = 6a*x + 2b]
        SplineSegment f = GetFunctionForX(x);
        return 6f * f.a * x + 2f * f.b;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || nodes == null || nodes.Count < 2 || functions.Count == 0) return;

        // Draw Spline Curve
        Gizmos.color = splineColor;
        for (int i = 0; i < functions.Count; i++)
        {
            //Define Domain and Step Increment For Current Function:
            float xStart = functions[i].xStart;
            float xEnd = functions[i].xEnd;
            float step = (xEnd - xStart) / resolution;

            //Define P(x) At Start Of Domain As Initial Position:
            Vector3 prevPt = new Vector3(xStart, Evaluate(xStart), 0f);

            for (int j = 1; j <= resolution; j++)
            {
                float x = xStart + j * step;
                Vector3 currentPt = new Vector3(x, Evaluate(x), 0f);
                Gizmos.DrawLine(prevPt, currentPt);

                //Draw Velocity Vectors (First Derivative):
                if (showTangents)
                {
                    float deriv = EvaluateFirstDerivative(x);
                    Vector3 tangentDir = new Vector3(1f, deriv, 0f).normalized;

                    Gizmos.color = velocityColor;
                    Gizmos.DrawLine(currentPt, currentPt + tangentDir * vectorScale);

                    Gizmos.color = splineColor;
                }

                //Draw Acceleration Vectors (Second Derivative):
                if (showAccelerations)
                {
                    float accel = EvaluateSecondDerivative(x);
                    Vector3 accelDir = new Vector3(0f, accel, 0f);

                    Gizmos.color = accelerationColor;
                    Gizmos.DrawLine(currentPt, currentPt + accelDir * vectorScale);

                    Gizmos.color = splineColor;
                }

                prevPt = currentPt;
            }
        }

        //Draw Node Point:
        Gizmos.color = nodeColor;
        foreach (var p in nodes)
        {
            Gizmos.DrawSphere(new Vector3(p.x, p.y, 0f), 0.15f);
        }
    }
}
