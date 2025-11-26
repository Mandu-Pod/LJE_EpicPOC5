using System;
using System.Collections.Generic;
using UnityEngine;

public class PaperController : SingletonObject<PaperController>
{
    public static event Action OnPaperFolded;
    public static event Action OnPaperTokenized;
    public static event Action OnPaperInitialized;

    [Header("Paper Settings")]
    [SerializeField] private float paperSize = 5f;
    [SerializeField] private Material paperFrontMaterial;
    [SerializeField] private Material paperBackMaterial;
    [SerializeField] private Transform meshTransform;
    [SerializeField] private Color paperFrontColor;
    [SerializeField] private Color paperBackColor;
    
    [Header("토큰화 설정")]
    [SerializeField] private float tokenizeThreshold = 0.1f;

    private Mesh paperMesh;
    private string paperName = "PaperMesh";
    private List<List<Vector2>> currentVerticesLayers = new();
    
    // 각 레이어가 접힌 상태인지 추적
    private List<bool> currentLayerFoldedStates = new();

    private bool isDragging = false;
    private float initialArea;
    private bool isTokenized = false;
    private bool isInitialized = false;
    
    // 종이 메시 오브젝트만 관리하는 리스트
    private List<GameObject> paperMeshObjects = new();

    // 접히는 원본 영역 저장 (polyB)
    private List<List<Vector2>> foldingSourceLayers = new();


    private Vector2[] squareVertices = new Vector2[]
    {
        new(-0.5f, -0.5f),
        new(0.5f, -0.5f),
        new(0.5f, 0.5f),
        new(-0.5f, 0.5f)
    };

    public float InitialArea => initialArea;
    public bool IsTokenized => isTokenized;
    public bool IsInitialized => isInitialized;
    
    public float CurrentArea => CalculateUnfoldedArea();
    public float AreaRatio => initialArea > 0 ? CurrentArea / initialArea : 0f;

    private Vector2 pointA;
    private Vector2 pointB;
    private List<List<Vector2>> newVerticesLayers = new();
    private List<List<Vector2>> flipedVerticesLayers = new();
    private List<bool> newLayerFoldedStates = new();
    
    // 접기 확정 시점 데이터 저장
    private List<List<Vector2>> confirmedFlipedVerticesLayers = new();
    private Vector2 confirmedFoldLinePoint;
    private Vector2 confirmedFoldLineDirection;

    protected override void Awake()
    {
        base.Awake();
        
        if (paperFrontMaterial != null)
            paperFrontMaterial.color = paperFrontColor;
        if (paperBackMaterial != null)
            paperBackMaterial.color = paperBackColor;
            
        InitializePaper();
    }

    private void InitializePaper()
    {
        CreateSquareMesh();
        initialArea = CurrentArea;
        isInitialized = true;
        Debug.Log($"[종이] 초기 면적: {initialArea}");
        OnPaperInitialized?.Invoke();
        OnPaperFolded?.Invoke();
    }

    void CreateSquareMesh()
    {
        GameObject meshObj = new(paperName + "0");
        meshObj.transform.parent = meshTransform.transform;
        MeshFilter thisMeshFilter = meshObj.AddComponent<MeshFilter>();
        MeshRenderer thisMeshRenderer = meshObj.AddComponent<MeshRenderer>();

        if (paperFrontMaterial != null)
            thisMeshRenderer.material = paperFrontMaterial;

        paperMesh = new Mesh();
        paperMesh.name = paperName + "0";

        Vector2[] scaledVertices = ScaleArray(squareVertices, paperSize);
        currentVerticesLayers.Add(new List<Vector2>(scaledVertices));
        currentLayerFoldedStates.Add(false);  // 초기 레이어는 접히지 않은 상태

        paperMesh.vertices = ToVector3(scaledVertices);
        paperMesh.triangles = GenerateConvexTriangles(squareVertices.Length);
        paperMesh.RecalculateNormals();
        paperMesh.RecalculateBounds();
        paperMesh.uv = CalculateUVsFromBounds(scaledVertices);

        thisMeshFilter.mesh = paperMesh;
        
        paperMeshObjects.Add(meshObj);
    }
    
    /// <summary>
    /// 접히지 않은 레이어들의 면적만 합산
    /// </summary>
    private float CalculateUnfoldedArea()
    {
        float totalArea = 0f;
        
        for (int i = 0; i < currentVerticesLayers.Count; i++)
        {
            // 접히지 않은 레이어만 면적 계산
            if (i < currentLayerFoldedStates.Count && !currentLayerFoldedStates[i])
            {
                totalArea += PaperAreaCalculator.CalculatePolygonArea(currentVerticesLayers[i]);
            }
        }
        
        // 초기 상태에서는 전체 면적 반환
        if (totalArea == 0f && currentVerticesLayers.Count > 0)
        {
            totalArea = PaperAreaCalculator.CalculatePolygonArea(currentVerticesLayers[0]);
        }
        
        return totalArea;
    }

    private void Update()
    {
        if (isTokenized) return;
        
        // 우클릭: 접기 취소
        if (Input.GetMouseButtonDown(1))
        {
            isDragging = false;
            UpdateMeshes(currentVerticesLayers, currentLayerFoldedStates);
            MarkManager.Instance?.RestoreMarkVisibility();
        }

        // 좌클릭 시작
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            pointA = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        // 좌클릭 종료: 접기 확정
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;

                if (newVerticesLayers.Count > 0)
                {
                    // 접기 확정 전에 데이터 저장
                    confirmedFlipedVerticesLayers = new List<List<Vector2>>();
                    foreach (var layer in flipedVerticesLayers)
                    {
                        confirmedFlipedVerticesLayers.Add(new List<Vector2>(layer));
                    }
                    
                    // 접는 선 정보 저장 (마크 반사 위치 계산용)
                    Vector2 midPoint = (pointA + pointB) / 2f;
                    Vector2 abDirection = (pointB - pointA).normalized;
                    confirmedFoldLinePoint = midPoint;
                    confirmedFoldLineDirection = new Vector2(-abDirection.y, abDirection.x);
                    
                    currentVerticesLayers = new List<List<Vector2>>(newVerticesLayers);
                    currentLayerFoldedStates = new List<bool>(newLayerFoldedStates);
                    
                    MarkManager.Instance?.ProcessFold();
                    
                    OnPaperFolded?.Invoke();
                    CheckTokenize();
                }
            }
        }

        if (isDragging)
        {
            UpdatePaperVisuals();
            MarkManager.Instance?.UpdateMarkVisibility();
        }
    }

    private void CheckTokenize()
    {
        float ratio = AreaRatio;
        Debug.Log($"[종이] 현재 면적 비율: {ratio:P1}");
        
        if (ratio <= tokenizeThreshold)
        {
            Tokenize();
        }
    }

    private void Tokenize()
    {
        isTokenized = true;
        Debug.Log("[종이] 토큰화! 종이가 토큰으로 변환됩니다.");
        
        MarkManager.Instance?.ClearAllMarks();
        
        foreach (var meshObj in paperMeshObjects)
        {
            if (meshObj != null)
                meshObj.SetActive(false);
        }
        
        OnPaperTokenized?.Invoke();
    }

    private void UpdatePaperVisuals()
    {
        pointB = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newVerticesLayers.Clear();
        flipedVerticesLayers.Clear();
        foldingSourceLayers.Clear();  // 추가
        newLayerFoldedStates.Clear();

        Vector2 midPoint = (pointA + pointB) / 2f;
        Vector2 abDirection = (pointB - pointA).normalized;
        Vector2 foldAxis = new(-abDirection.y, abDirection.x);

        if (abDirection == Vector2.zero)
            return;

        foreach (List<Vector2> layer in currentVerticesLayers)
        {
            List<Vector2> polyA, polyB, flipedPolyB;
            SplitPolygonByLine(layer, midPoint, abDirection, foldAxis, out polyA, out polyB, out flipedPolyB);
            if (polyA.Count > 2)
            {
                newVerticesLayers.Add(polyA);
                newLayerFoldedStates.Add(false);
            }
            if (flipedPolyB.Count > 2)
            {
                newVerticesLayers.Add(flipedPolyB);
                newLayerFoldedStates.Add(true);
                foldingSourceLayers.Add(polyB);      // polyB 저장
                flipedVerticesLayers.Add(flipedPolyB);
            }
        }

        UpdateMeshes(newVerticesLayers, newLayerFoldedStates);
    }
    private void UpdateMeshes(List<List<Vector2>> verticesLayers, List<bool> layerFolded)
    {
        // 필요한 만큼 메시 오브젝트 생성
        while (paperMeshObjects.Count < verticesLayers.Count)
        {
            int index = paperMeshObjects.Count;
            GameObject meshObj = new(paperName + index);
            meshObj.transform.parent = meshTransform.transform;
            meshObj.AddComponent<MeshFilter>();
            MeshRenderer thisMeshRenderer = meshObj.AddComponent<MeshRenderer>();

            if (paperFrontMaterial != null)
                thisMeshRenderer.material = paperFrontMaterial;
                
            paperMeshObjects.Add(meshObj);
        }

        // 메시 업데이트
        for (int i = 0; i < paperMeshObjects.Count; i++)
        {
            GameObject meshObj = paperMeshObjects[i];
            if (meshObj == null) continue;
            
            MeshFilter thisMeshFilter = meshObj.GetComponent<MeshFilter>();
            if (thisMeshFilter == null) continue;

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
                meshObj.SetActive(true);
                
                MeshRenderer thisMeshRenderer = meshObj.GetComponent<MeshRenderer>();
                if (thisMeshRenderer != null)
                {
                    bool isFolded = i < layerFolded.Count && layerFolded[i];
                    thisMeshRenderer.material = isFolded ? paperFrontMaterial : paperBackMaterial;
                }
            }
            else
            {
                meshObj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 접히는 원본 영역(polyB)에 있는지 체크 - 마크가 접는 선을 넘어갔는지
    /// </summary>
    public bool IsPointInsideFoldingSource(Vector2 point)
    {
        if (foldingSourceLayers == null || foldingSourceLayers.Count == 0)
            return false;
        return PaperRandomUtility.IsPointInsidePolygons(point, foldingSourceLayers);
    }

    public Vector2 GetRandomInternalPoint()
    {
        return PaperRandomUtility.GetRandomPointOnPolygons(currentVerticesLayers);
    }

    /// <summary>
    /// 드래그 중 실시간 겹침 판정용
    /// </summary>
    public bool IsPointInsideFlipedPolygons(Vector2 point)
    {
        if (flipedVerticesLayers == null || flipedVerticesLayers.Count == 0)
            return false;
        return PaperRandomUtility.IsPointInsidePolygons(point, flipedVerticesLayers);
    }
    
    /// <summary>
    /// 접기 확정 후 겹침 판정용
    /// </summary>
    public bool IsPointInsideConfirmedFlipedPolygons(Vector2 point)
    {
        if (confirmedFlipedVerticesLayers == null || confirmedFlipedVerticesLayers.Count == 0)
            return false;
        return PaperRandomUtility.IsPointInsidePolygons(point, confirmedFlipedVerticesLayers);
    }
    
    /// <summary>
    /// 접기 확정 후 마크의 반사 위치 계산
    /// </summary>
    public Vector2 GetReflectedPosition(Vector2 originalPosition)
    {
        return ReflectPointAcrossLine(confirmedFoldLinePoint, confirmedFoldLineDirection, originalPosition);
    }
    
    /// <summary>
    /// 드래그 중 실시간 반사 위치 계산
    /// </summary>
    public Vector2 GetReflectedPositionRealtime(Vector2 originalPosition)
    {
        Vector2 midPoint = (pointA + pointB) / 2f;
        Vector2 abDirection = (pointB - pointA).normalized;
        Vector2 foldAxis = new(-abDirection.y, abDirection.x);
        
        return ReflectPointAcrossLine(midPoint, foldAxis, originalPosition);
    }

    // ============ 유틸리티 함수들 ============

    private Vector2 ReflectPointAcrossLine(Vector2 p0, Vector2 dir, Vector2 point)
    {
        Vector2 n = dir.normalized;
        Vector2 v = point - p0;
        float projScalar = Vector2.Dot(v, n);
        Vector2 proj = n * projScalar;
        Vector2 perp = v - proj;
        return point - 2f * perp;
    }

    private int GetSide(Vector2 point, Vector2 linePoint, Vector2 lineNormal)
    {
        Vector2 vecToPoint = point - linePoint;
        float dot = Vector2.Dot(vecToPoint, lineNormal);
        if (Mathf.Approximately(dot, 0f))
            return 0;
        if (dot < 0)
            return -1;
        return 1;
    }

    private void SplitPolygonByLine(List<Vector2> polygon, Vector2 linePoint, Vector2 lineNormal, Vector2 lineDirection,
                                    out List<Vector2> polygonA, out List<Vector2> polygonB, out List<Vector2> flipedPolygonB)
    {
        polygonA = new List<Vector2>();
        polygonB = new List<Vector2>();
        flipedPolygonB = new List<Vector2>();

        if (polygon == null || polygon.Count < 3)
            return;

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
                    flipedPolygonB.Add(intersection);
                }
            }
        }
    }

    private bool LineSegmentIntersection(Vector2 linePoint, Vector2 lineDir, Vector2 segA, Vector2 segB, out Vector2 hitPoint)
    {
        hitPoint = Vector2.zero;

        Vector2 v1 = linePoint - segA;
        Vector2 v2 = segB - segA;
        Vector2 v3 = new Vector2(-lineDir.y, lineDir.x);

        float dot = Vector2.Dot(v2, v3);

        if (Mathf.Abs(dot) < Mathf.Epsilon)
            return false;

        float t1 = Cross(v2, v1) / dot;
        float t2 = Vector2.Dot(v1, v3) / dot;

        if (t2 < 0f || t2 > 1f)
            return false;

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
            return new Vector2[0];

        float minX = vertices[0].x, minY = vertices[0].y;
        float maxX = vertices[0].x, maxY = vertices[0].y;

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
            return new int[0];

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
}
