using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PaperController : MonoBehaviour
{
    [Header("Paper Settings")]
    [SerializeField] private float paperSize = 10f;
    [SerializeField] private Material paperMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private PolygonCollider2D polyCollider;
    private Mesh paperMesh;

    private Vector2 dragStartPoint;
    private bool isDragging = false;
    private Camera mainCamera;

    private List<Vector2> vertices = new List<Vector2>();
    private List<Vector2> originalVertices = new List<Vector2>(); // 드래그 시작 시 원본
    private List<GameObject> foldedLayers = new List<GameObject>(); // 접힌 레이어들
    private GameObject previewLayer; // 미리보기 레이어

    void Start()
    {
        mainCamera = Camera.main;

        // Setup mesh components
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        if (polyCollider == null)
            polyCollider = gameObject.AddComponent<PolygonCollider2D>();

        if (paperMaterial != null)
            meshRenderer.material = paperMaterial;
        else
        {
            paperMaterial = new Material(Shader.Find("Sprites/Default"));
            paperMaterial.color = Color.white;
            meshRenderer.material = paperMaterial;
        }

        InitializePaper();
    }

    void InitializePaper()
    {
        paperMesh = new Mesh();
        paperMesh.name = "Paper";

        // Create a square paper
        vertices.Clear();

        float half = paperSize / 2f;

        vertices.Add(new Vector2(-half, -half));
        vertices.Add(new Vector2(half, -half));
        vertices.Add(new Vector2(half, half));
        vertices.Add(new Vector2(-half, half));

        UpdateMesh();
        UpdateCollider();

        Debug.Log("Paper initialized");
    }

    void UpdateMesh()
    {
        if (paperMesh == null)
        {
            paperMesh = new Mesh();
            paperMesh.name = "Paper";
        }

        paperMesh.Clear();

        // Convert Vector2 to Vector3
        Vector3[] vertices3D = new Vector3[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices3D[i] = new Vector3(vertices[i].x, vertices[i].y, 0);
        }

        // Create triangles (fan triangulation)
        int[] triangles = new int[(vertices.Count - 2) * 3];
        for (int i = 0; i < vertices.Count - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        paperMesh.vertices = vertices3D;
        paperMesh.triangles = triangles;
        paperMesh.RecalculateNormals();
        paperMesh.RecalculateBounds();

        meshFilter.mesh = paperMesh;
    }

    void UpdateCollider()
    {
        polyCollider.points = vertices.ToArray();
    }

    void Update()
    {
        HandleDragInput();
    }

    void HandleDragInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                dragStartPoint = hit.point;
                originalVertices = new List<Vector2>(vertices); // 원본 저장
                Debug.Log($"Started dragging at: {dragStartPoint}");
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            // 실시간 미리보기 (메쉬만 업데이트)
            Vector2 currentPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            FoldPaper(dragStartPoint, currentPoint, false); // 미리보기 모드
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Vector2 dragEndPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log($"Ended dragging at: {dragEndPoint}");

            // 최종 접기 적용 (콜라이더도 업데이트)
            FoldPaper(dragStartPoint, dragEndPoint, true); // 최종 모드

            isDragging = false;
            originalVertices.Clear();
        }

        // Visual feedback during drag
        if (isDragging)
        {
            Vector2 currentPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Debug.DrawLine(dragStartPoint, currentPoint, Color.yellow);
        }
    }

    void FoldPaper(Vector2 startPoint, Vector2 endPoint, bool isFinal)
    {
        // 미리보기일 때는 원본으로 복원
        if (!isFinal)
        {
            // 기존 미리보기 레이어 제거
            if (previewLayer != null)
            {
                Destroy(previewLayer);
            }
            vertices = new List<Vector2>(originalVertices);
        }

        // Convert to local space
        startPoint = transform.InverseTransformPoint(startPoint);
        endPoint = transform.InverseTransformPoint(endPoint);

        // If points are too close, don't fold
        if (Vector2.Distance(startPoint, endPoint) < 0.1f)
        {
            if (!isFinal)
            {
                UpdateMesh(); // 미리보기: 메쉬만
            }
            return;
        }

        // Calculate fold line: perpendicular bisector
        Vector2 midPoint = (startPoint + endPoint) / 2f;
        Vector2 connectingLine = (endPoint - startPoint).normalized;
        Vector2 foldLineNormal = new Vector2(-connectingLine.y, connectingLine.x);

        // 1. 접힐 부분(startPoint 쪽)과 남을 부분을 분리
        List<Vector2> foldedVertices = new List<Vector2>();
        List<Vector2> remainingVertices = new List<Vector2>();
        List<Vector2> intersectionPoints = new List<Vector2>();

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 vertex = vertices[i];
            Vector2 toVertex = vertex - midPoint;
            float sideDistance = Vector2.Dot(toVertex, foldLineNormal);

            Vector2 toStart = startPoint - midPoint;
            float startSide = Vector2.Dot(toStart, foldLineNormal);

            if (Mathf.Sign(sideDistance) == Mathf.Sign(startSide))
            {
                foldedVertices.Add(vertex);
            }
            else
            {
                remainingVertices.Add(vertex);
            }

            // 선분이 fold line과 교차하는지 확인
            int nextIdx = (i + 1) % vertices.Count;
            Vector2 nextVertex = vertices[nextIdx];
            Vector2 toNext = nextVertex - midPoint;
            float nextSide = Vector2.Dot(toNext, foldLineNormal);

            if (Mathf.Sign(sideDistance) != Mathf.Sign(nextSide) && Mathf.Abs(sideDistance) > 0.01f && Mathf.Abs(nextSide) > 0.01f)
            {
                float t = sideDistance / (sideDistance - nextSide);
                Vector2 intersection = vertex + t * (nextVertex - vertex);
                intersectionPoints.Add(intersection);
            }
        }

        if (foldedVertices.Count == 0)
        {
            if (!isFinal)
            {
                UpdateMesh(); // 미리보기: 메쉬만
            }
            return;
        }

        // 2. 접힌 부분을 반사시켜서 레이어 생성
        GameObject foldedLayer = new GameObject(isFinal ? "FoldedLayer" : "PreviewLayer");
        foldedLayer.transform.SetParent(transform);
        foldedLayer.transform.localPosition = Vector3.zero;
        foldedLayer.transform.localRotation = Quaternion.identity;
        foldedLayer.transform.localScale = Vector3.one;

        MeshFilter layerMeshFilter = foldedLayer.AddComponent<MeshFilter>();
        MeshRenderer layerMeshRenderer = foldedLayer.AddComponent<MeshRenderer>();

        // 최종일 때만 콜라이더 추가
        PolygonCollider2D layerCollider = null;
        if (isFinal)
        {
            layerCollider = foldedLayer.AddComponent<PolygonCollider2D>();
        }

        // 미리보기는 반투명
        if (!isFinal)
        {
            Material previewMaterial = new Material(paperMaterial);
            Color previewColor = previewMaterial.color;
            previewColor.a = 0.7f;
            previewMaterial.color = previewColor;
            layerMeshRenderer.material = previewMaterial;
            previewLayer = foldedLayer;
        }
        else
        {
            layerMeshRenderer.material = paperMaterial;
        }

        // 반사된 꼭지점 생성
        List<Vector2> reflectedVertices = new List<Vector2>();
        foreach (Vector2 v in foldedVertices)
        {
            Vector2 toV = v - midPoint;
            float dist = Vector2.Dot(toV, foldLineNormal);
            Vector2 reflected = v - 2f * dist * foldLineNormal;
            reflectedVertices.Add(reflected);
        }

        reflectedVertices.AddRange(intersectionPoints);

        if (reflectedVertices.Count >= 3)
        {
            // 메쉬 생성
            Mesh layerMesh = new Mesh();
            layerMesh.name = isFinal ? "FoldedLayer" : "PreviewLayer";

            Vector3[] layerVertices3D = new Vector3[reflectedVertices.Count];
            for (int i = 0; i < reflectedVertices.Count; i++)
            {
                layerVertices3D[i] = new Vector3(reflectedVertices[i].x, reflectedVertices[i].y, 0.01f);
            }

            int[] layerTriangles = new int[(reflectedVertices.Count - 2) * 3];
            for (int i = 0; i < reflectedVertices.Count - 2; i++)
            {
                layerTriangles[i * 3] = 0;
                layerTriangles[i * 3 + 1] = i + 1;
                layerTriangles[i * 3 + 2] = i + 2;
            }

            layerMesh.vertices = layerVertices3D;
            layerMesh.triangles = layerTriangles;
            layerMesh.RecalculateNormals();
            layerMesh.RecalculateBounds();

            layerMeshFilter.mesh = layerMesh;

            // 최종일 때만 콜라이더 설정
            if (isFinal && layerCollider != null)
            {
                layerCollider.points = reflectedVertices.ToArray();
                foldedLayers.Add(foldedLayer);
            }
        }

        // 3. 기존 종이는 남은 부분 + 교차점으로 갱신
        remainingVertices.AddRange(intersectionPoints);
        vertices = remainingVertices;

        UpdateMesh(); // 메쉬는 항상 업데이트

        // 콜라이더는 최종일 때만 업데이트
        if (isFinal)
        {
            UpdateCollider();

            // Notify game manager
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnPaperFolded();
            }

            Debug.Log("Paper folded successfully!");
        }
    }

    public Bounds GetPaperBounds()
    {
        if (meshFilter != null && meshFilter.mesh != null)
        {
            return meshFilter.mesh.bounds;
        }
        return new Bounds(Vector3.zero, Vector3.one * paperSize);
    }

    public Vector3 GetRandomPositionOnPaper()
    {
        Bounds bounds = GetPaperBounds();
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        return transform.TransformPoint(new Vector3(x, y, 0.1f));
    }
}
