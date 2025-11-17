using System;
using System.Collections.Generic;
using UnityEngine;

public class PaperController : SingletonObject<PaperController>
{
    public static event Action OnPaperFolded;

    [Header("Paper Settings")]
    [SerializeField] private float paperSize = 5f;
    [SerializeField] private Material paperFrontMaterial;
    [SerializeField] private Material paperBackMaterial;
    [SerializeField] private Transform meshTransform;

    [SerializeField] private Color paperFrontColor;
    [SerializeField] private Color paperBackColor;


    private Mesh paperMesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private string paperName = "PaperMesh";
    private List<List<Vector2>> currentVerticesLayers = new();

    private bool isDragging = false;

    private Vector2[] squareVertices = new Vector2[]
    {
        new(-0.5f, -0.5f),
        new(0.5f, -0.5f),
        new(0.5f, 0.5f),
        new(-0.5f, 0.5f)
    };

    private void InitializePaper()
    {
        CreateSquareMesh();
        OnPaperFolded?.Invoke();
    }

    void CreateSquareMesh()
    {
        GameObject meshObj = new(paperName + "0");
        meshObj.transform.parent = meshTransform.transform;
        MeshFilter thisMeshFilter = meshObj.AddComponent<MeshFilter>();
        MeshRenderer thisMeshRenderer = meshObj.AddComponent<MeshRenderer>();
        // PolygonCollider2D thisPolygonCollider = meshObj.AddComponent<PolygonCollider2D>();

        if (paperFrontMaterial != null)
            thisMeshRenderer.material = paperFrontMaterial;

        paperMesh = new Mesh();
        paperMesh.name = paperName + "0";

        Vector2[] scaledVertices = ScaleArray(squareVertices, paperSize);
        currentVerticesLayers.Add(new List<Vector2>(scaledVertices));

        paperMesh.vertices = ToVector3(scaledVertices);
        paperMesh.triangles = GenerateConvexTriangles(squareVertices.Length);
        paperMesh.RecalculateNormals();
        paperMesh.RecalculateBounds();

        paperMesh.uv = CalculateUVsFromBounds(scaledVertices);


        thisMeshFilter.mesh = paperMesh;
        // thisPolygonCollider.points = scaledVertices;
    }

    protected override void Awake()
    {
        base.Awake();
        paperFrontMaterial.color = paperFrontColor;
        paperBackMaterial.color = paperBackColor;
        InitializePaper();
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isDragging = false;
            UpdateMeshes(currentVerticesLayers, new List<bool>(new bool[currentVerticesLayers.Count]));
        }

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            pointA = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;
                currentVerticesLayers = new List<List<Vector2>>(newVerticesLayers);
                OnPaperFolded?.Invoke();
            }
        }

        if (isDragging)
        {
            UpdatePaperVisuals();
        }
    }

    private Vector2 pointA;
    private Vector2 pointB;
    private List<List<Vector2>> newVerticesLayers = new();
    private List<List<Vector2>> flipedVerticesLayers = new();

    private void UpdatePaperVisuals()
    {
        pointB = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newVerticesLayers.Clear();
        flipedVerticesLayers.Clear();
        List<bool> layerFolded = new();
        Vector2 midPoint = (pointA + pointB) / 2f;
        Vector2 abDirection = (pointB - pointA).normalized;
        Vector2 foldAxis = new(-abDirection.y, abDirection.x);

        if (abDirection == Vector2.zero)
        {
            return;
        }
        foreach (List<Vector2> layer in currentVerticesLayers)
        {
            List<Vector2> polyA, polyB, flipedPolyB;
            SplitPolygonByLine(layer, midPoint, abDirection, foldAxis, out polyA, out polyB, out flipedPolyB);
            if (polyA.Count > 2)
            {
                newVerticesLayers.Add(polyA);
                layerFolded.Add(false);
            }
            if (flipedPolyB.Count > 2)
            {
                newVerticesLayers.Add(flipedPolyB);
                layerFolded.Add(true);
                flipedVerticesLayers.Add(polyB);
                flipedVerticesLayers.Add(flipedPolyB);

            }
        }

        UpdateMeshes(newVerticesLayers, layerFolded);
    }


    private void UpdateMeshes(List<List<Vector2>> verticesLayers, List<bool> layerFolded)
    {
        if (meshTransform.childCount < verticesLayers.Count)
        {
            for (int i = meshTransform.childCount; i < verticesLayers.Count; i++)
            {
                GameObject meshObj = new(paperName + i);
                meshObj.transform.parent = meshTransform.transform;
                meshObj.AddComponent<MeshFilter>();
                MeshRenderer thisMeshRenderer = meshObj.AddComponent<MeshRenderer>();

                if (paperFrontMaterial != null)
                    thisMeshRenderer.material = paperFrontMaterial;

            }
        }

        for (int i = 0; i < meshTransform.childCount; i++)
        {
            Transform child = meshTransform.GetChild(i);
            MeshFilter thisMeshFilter = child.GetComponent<MeshFilter>();

            if (i < verticesLayers.Count)
            {
                Vector2[] layerVertices = verticesLayers[i].ToArray();

                Mesh layerMesh = new Mesh();
                layerMesh.name = paperName + i;
                layerMesh.vertices = ToVector3(layerVertices);
                layerMesh.triangles = GenerateConvexTriangles(layerVertices.Length);
                layerMesh.RecalculateNormals();
                layerMesh.RecalculateBounds();

                thisMeshFilter.mesh = layerMesh;
                child.gameObject.SetActive(true);
                MeshRenderer thisMeshRenderer = child.GetComponent<MeshRenderer>();
                if (layerFolded[i])
                {
                    if (paperFrontMaterial != null)
                        thisMeshRenderer.material = paperFrontMaterial;
                }
                else
                {
                    if (paperBackMaterial != null)
                        thisMeshRenderer.material = paperBackMaterial;
                }
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }


    public Vector2 GetRandomInternalPoint()
    {
        return PaperRandomUtility.GetRandomPointOnPolygons(currentVerticesLayers);
    }

    public bool IsPointInsideFlipedPolygons(Vector2 point)
    {
        return PaperRandomUtility.IsPointInsidePolygons(point, flipedVerticesLayers);
    }

    // 유틸리티 함수들 ---------------------------------


    private Vector2 ReflectPointAcrossLine(Vector2 p0, Vector2 dir, Vector2 point)
    {
        Vector2 n = dir.normalized; // direction of the line

        Vector2 v = point - p0;              // from line point to target point
        float projScalar = Vector2.Dot(v, n);
        Vector2 proj = n * projScalar;        // projection onto line
        Vector2 perp = v - proj;              // perpendicular component

        return point - 2f * perp;             // reflect
    }

    // 점이 선의 어느 쪽에 있는지 판별합니다.
    private int GetSide(Vector2 point, Vector2 linePoint, Vector2 lineNormal)
    {
        Vector2 vecToPoint = point - linePoint;
        float dot = Vector2.Dot(vecToPoint, lineNormal);
        if (Mathf.Approximately(dot, 0f))
        {
            return 0;
        }

        if (dot < 0)
        {
            return -1;
        }

        return 1;
    }

    ///하나의 폴리곤을 무한한 선을 기준으로 두 개의 폴리곤(A, B)으로 분할합니다.
    private void SplitPolygonByLine(List<Vector2> polygon, Vector2 linePoint, Vector2 lineNormal, Vector2 lineDirection,
                                    out List<Vector2> polygonA, out List<Vector2> polygonB, out List<Vector2> flipedPolygonB)
    {
        polygonA = new List<Vector2>();
        polygonB = new List<Vector2>();
        flipedPolygonB = new List<Vector2>();

        if (polygon == null || polygon.Count < 3)
        {
            return;
        }

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 p1 = polygon[i];
            Vector2 p2 = polygon[(i + 1) % polygon.Count];

            int p1Side = GetSide(p1, linePoint, lineNormal);
            int p2Side = GetSide(p2, linePoint, lineNormal);

            if (p1Side == -1)
            {
                polygonB.Add(p1);
                Vector2 reflectedPoint = ReflectPointAcrossLine(linePoint, lineDirection, p1);
                flipedPolygonB.Add(reflectedPoint);
            }
            else if (p1Side == 1)
            {
                polygonA.Add(p1);
            }
            else
            {
                polygonB.Add(p1);
                polygonA.Add(p1);
            }

            if (p1Side * p2Side < 0)
            {
                if (LineSegmentIntersection(linePoint, lineDirection, p1, p2, out Vector2 intersection))
                {
                    polygonB.Add(intersection);
                    polygonA.Add(intersection);
                }
            }
        }
    }

    private bool LineSegmentIntersection(Vector2 linePoint, Vector2 lineDir, Vector2 segA, Vector2 segB, out Vector2 hitPoint)
    {
        hitPoint = Vector2.zero;

        Vector2 v1 = linePoint - segA;
        Vector2 v2 = segB - segA;
        Vector2 v3 = new Vector2(-lineDir.y, lineDir.x); // lineDir의 수직 벡터

        float dot = Vector2.Dot(v2, v3);

        // dot == 0 → 평행 (교차 X 또는 무한히 겹침)
        if (Mathf.Abs(dot) < Mathf.Epsilon)
            return false;

        float t1 = Cross(v2, v1) / dot;     // line param
        float t2 = Vector2.Dot(v1, v3) / dot; // segment param (0~1 → 선분 내부)

        if (t2 < 0f || t2 > 1f)
            return false; // 선분 범위 밖

        hitPoint = linePoint + lineDir * t1;
        return true;
    }

    private float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private Vector2[] CalculateUVsFromBounds(Vector2[] vertices)
    {
        if (vertices == null || vertices.Length < 3)
        {
            return new Vector2[0];
        }

        float minX = vertices[0].x;
        float minY = vertices[0].y;
        float maxX = vertices[0].x;
        float maxY = vertices[0].y;

        for (int i = 1; i < vertices.Length; i++)
        {
            if (vertices[i].x < minX) minX = vertices[i].x;
            if (vertices[i].x > maxX) maxX = vertices[i].x;
            if (vertices[i].y < minY) minY = vertices[i].y;
            if (vertices[i].y > maxY) maxY = vertices[i].y;
        }

        float width = maxX - minX;
        float height = maxY - minY;

        bool widthIsZero = Mathf.Approximately(width, 0f);
        bool heightIsZero = Mathf.Approximately(height, 0f);

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            float uvX = widthIsZero ? 0.5f : (vertices[i].x - minX) / width;
            float uvY = heightIsZero ? 0.5f : (vertices[i].y - minY) / height;
            uvs[i] = new Vector2(uvX, uvY);
        }

        return uvs;
    }
    private int[] GenerateConvexTriangles(int vertexCount)
    {
        if (vertexCount < 3)
        {
            return new int[0];
        }

        int[] triangles = new int[(vertexCount - 2) * 3];
        int triangleIndex = 0;

        for (int i = 0; i < vertexCount - 2; i++)
        {
            triangles[triangleIndex + 0] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = i + 2;

            triangleIndex += 3;
        }

        return triangles;
    }

    private Vector3[] ToVector3(Vector2[] v2Array, float z = 0f)
    {
        Vector3[] v3Array = new Vector3[v2Array.Length];
        for (int i = 0; i < v2Array.Length; i++)
        {
            v3Array[i] = new Vector3(v2Array[i].x, v2Array[i].y, z);
        }
        return v3Array;
    }

    private Vector2[] ScaleArray(Vector2[] v2Array, float scale)
    {
        Vector2[] scaledArray = new Vector2[v2Array.Length];

        for (int i = 0; i < v2Array.Length; i++)
        {
            scaledArray[i] = v2Array[i] * scale;
        }

        return scaledArray;
    }

    private Vector3[] ScaleArray(Vector3[] v3Array, float scale)
    {
        Vector3[] scaledArray = new Vector3[v3Array.Length];
        for (int i = 0; i < v3Array.Length; i++)
        {
            scaledArray[i] = v3Array[i] * scale;
        }
        return scaledArray;
    }
}
