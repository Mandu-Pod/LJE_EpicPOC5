using UnityEngine;
using UnityEditor;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class FoldlandsSetupEditor : EditorWindow
{
    private const string MENU_PATH = "Tools/Foldlands/";

    [MenuItem(MENU_PATH + "전체 셋업 (Full Setup)")]
    public static void FullSetup()
    {
        if (!EditorUtility.DisplayDialog("Foldlands 셋업",
            "씬에 모든 오브젝트를 자동으로 생성합니다.\n\n" +
            "- 매니저 오브젝트들\n" +
            "- 종이 오브젝트\n" +
            "- UI 캔버스\n" +
            "- 머티리얼\n" +
            "- 마크 프리팹\n" +
            "- 레시피 데이터\n\n" +
            "진행하시겠습니까?",
            "확인", "취소"))
        {
            return;
        }

        CreateFolders();
        CreateMaterials();
        CreateMarkPrefabs();
        CreateRecipeData();
        CreateManagers();
        CreatePaper();
        CreateUI();

        Debug.Log("[Foldlands] 셋업 완료!");
        EditorUtility.DisplayDialog("완료", "Foldlands 셋업이 완료되었습니다!", "확인");
    }

    [MenuItem(MENU_PATH + "1. 폴더 생성")]
    public static void CreateFolders()
    {
        string[] folders = new string[]
        {
            "Assets/Foldlands",
            "Assets/Foldlands/Materials",
            "Assets/Foldlands/Prefabs",
            "Assets/Foldlands/ScriptableObjects",
            "Assets/Foldlands/Sprites"
        };

        foreach (string folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = Path.GetDirectoryName(folder);
                string newFolder = Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, newFolder);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("[Foldlands] 폴더 생성 완료");
    }

    [MenuItem(MENU_PATH + "2. 머티리얼 생성")]
    public static void CreateMaterials()
    {
        CreateFolders();

        // 종이 앞면
        Material frontMat = new Material(Shader.Find("Sprites/Default"));
        frontMat.color = new Color(0.95f, 0.9f, 0.8f);
        AssetDatabase.CreateAsset(frontMat, "Assets/Foldlands/Materials/PaperFront.mat");

        // 종이 뒷면
        Material backMat = new Material(Shader.Find("Sprites/Default"));
        backMat.color = new Color(0.8f, 0.75f, 0.65f);
        AssetDatabase.CreateAsset(backMat, "Assets/Foldlands/Materials/PaperBack.mat");

        // 나무 마크
        Material treeMat = new Material(Shader.Find("Sprites/Default"));
        treeMat.color = new Color(0.2f, 0.6f, 0.2f);
        AssetDatabase.CreateAsset(treeMat, "Assets/Foldlands/Materials/TreeMark.mat");

        // 돌 마크
        Material stoneMat = new Material(Shader.Find("Sprites/Default"));
        stoneMat.color = new Color(0.5f, 0.5f, 0.5f);
        AssetDatabase.CreateAsset(stoneMat, "Assets/Foldlands/Materials/StoneMark.mat");

        // 식량 마크
        Material foodMat = new Material(Shader.Find("Sprites/Default"));
        foodMat.color = new Color(0.9f, 0.7f, 0.3f);
        AssetDatabase.CreateAsset(foodMat, "Assets/Foldlands/Materials/FoodMark.mat");

        // 인구 마크
        Material personMat = new Material(Shader.Find("Sprites/Default"));
        personMat.color = new Color(0.3f, 0.5f, 0.9f);
        AssetDatabase.CreateAsset(personMat, "Assets/Foldlands/Materials/PersonMark.mat");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Foldlands] 머티리얼 생성 완료");
    }

    [MenuItem(MENU_PATH + "3. 마크 프리팹 생성")]
    public static void CreateMarkPrefabs()
    {
        CreateFolders();
        CreateMaterials();

        // 나무
        GameObject treeObj = CreateMarkObject("TreeMark", "Assets/Foldlands/Materials/TreeMark.mat");
        PrefabUtility.SaveAsPrefabAsset(treeObj, "Assets/Foldlands/Prefabs/TreeMark.prefab");
        Object.DestroyImmediate(treeObj);

        // 돌
        GameObject stoneObj = CreateMarkObject("StoneMark", "Assets/Foldlands/Materials/StoneMark.mat");
        PrefabUtility.SaveAsPrefabAsset(stoneObj, "Assets/Foldlands/Prefabs/StoneMark.prefab");
        Object.DestroyImmediate(stoneObj);

        // 식량
        GameObject foodObj = CreateMarkObject("FoodMark", "Assets/Foldlands/Materials/FoodMark.mat");
        PrefabUtility.SaveAsPrefabAsset(foodObj, "Assets/Foldlands/Prefabs/FoodMark.prefab");
        Object.DestroyImmediate(foodObj);

        // 인구
        GameObject personObj = CreateMarkObject("PersonMark", "Assets/Foldlands/Materials/PersonMark.mat");
        PrefabUtility.SaveAsPrefabAsset(personObj, "Assets/Foldlands/Prefabs/PersonMark.prefab");
        Object.DestroyImmediate(personObj);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Foldlands] 마크 프리팹 생성 완료");
    }

    private static GameObject CreateMarkObject(string name, string materialPath)
    {
        GameObject obj = new GameObject(name);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (mat != null)
        {
            sr.material = mat;
            sr.color = mat.color;
        }

        obj.AddComponent<Mark>();

        return obj;
    }

    private static Sprite CreateCircleSprite()
    {
        string spritePath = "Assets/Foldlands/Sprites/Circle.png";

        if (!File.Exists(spritePath))
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];

            float center = size / 2f;
            float radius = size / 2f - 1;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    pixels[y * size + x] = dist <= radius ? Color.white : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            byte[] pngData = tex.EncodeToPNG();
            File.WriteAllBytes(spritePath, pngData);
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
    }

    [MenuItem(MENU_PATH + "4. 레시피 데이터 생성")]
    public static void CreateRecipeData()
    {
        CreateFolders();

        // 나무 + 사람 = 목재
        RecipeData treePersonRecipe = ScriptableObject.CreateInstance<RecipeData>();
        treePersonRecipe.ingredient1 = MarkType.Tree;
        treePersonRecipe.ingredient2 = MarkType.Person;
        treePersonRecipe.result = MarkType.Wood;
        treePersonRecipe.description = "나무를 사람이 베어 목재를 얻습니다.";
        AssetDatabase.CreateAsset(treePersonRecipe, "Assets/Foldlands/ScriptableObjects/Recipe_TreePerson.asset");

        // 레시피 데이터베이스
        RecipeDatabase database = ScriptableObject.CreateInstance<RecipeDatabase>();
        database.recipes.Add(treePersonRecipe);
        AssetDatabase.CreateAsset(database, "Assets/Foldlands/ScriptableObjects/RecipeDatabase.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Foldlands] 레시피 데이터 생성 완료");
    }

    [MenuItem(MENU_PATH + "5. 매니저 오브젝트 생성")]
    public static void CreateManagers()
    {
        DestroyExistingManager<GameManager>();
        DestroyExistingManager<TurnSystem>();
        DestroyExistingManager<PopulationManager>();
        DestroyExistingManager<InventorySystem>();
        DestroyExistingManager<MarkManager>();

        GameObject gameManagerObj = new GameObject("GameManager");
        gameManagerObj.AddComponent<GameManager>();

        GameObject turnSystemObj = new GameObject("TurnSystem");
        turnSystemObj.AddComponent<TurnSystem>();

        GameObject populationObj = new GameObject("PopulationManager");
        populationObj.AddComponent<PopulationManager>();

        GameObject inventoryObj = new GameObject("InventorySystem");
        inventoryObj.AddComponent<InventorySystem>();

        GameObject markManagerObj = new GameObject("MarkManager");
        MarkManager markManager = markManagerObj.AddComponent<MarkManager>();

        SerializedObject markManagerSO = new SerializedObject(markManager);

        GameObject treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Foldlands/Prefabs/TreeMark.prefab");
        GameObject stonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Foldlands/Prefabs/StoneMark.prefab");
        GameObject foodPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Foldlands/Prefabs/FoodMark.prefab");
        GameObject personPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Foldlands/Prefabs/PersonMark.prefab");
        RecipeDatabase recipeDB = AssetDatabase.LoadAssetAtPath<RecipeDatabase>("Assets/Foldlands/ScriptableObjects/RecipeDatabase.asset");

        markManagerSO.FindProperty("treePrefab").objectReferenceValue = treePrefab;
        markManagerSO.FindProperty("stonePrefab").objectReferenceValue = stonePrefab;
        markManagerSO.FindProperty("foodPrefab").objectReferenceValue = foodPrefab;
        markManagerSO.FindProperty("personPrefab").objectReferenceValue = personPrefab;
        markManagerSO.FindProperty("recipeDatabase").objectReferenceValue = recipeDB;
        markManagerSO.ApplyModifiedProperties();

        Debug.Log("[Foldlands] 매니저 오브젝트 생성 완료");
    }

    [MenuItem(MENU_PATH + "6. 종이 오브젝트 생성")]
    public static void CreatePaper()
    {
        DestroyExistingManager<PaperController>();

        GameObject paperRoot = new GameObject("Paper");
        PaperController paperController = paperRoot.AddComponent<PaperController>();

        GameObject meshTransform = new GameObject("MeshTransform");
        meshTransform.transform.parent = paperRoot.transform;

        GameObject markTransform = new GameObject("MarkTransform");
        markTransform.transform.parent = paperRoot.transform;

        SerializedObject paperSO = new SerializedObject(paperController);

        Material frontMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Foldlands/Materials/PaperFront.mat");
        Material backMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Foldlands/Materials/PaperBack.mat");

        paperSO.FindProperty("meshTransform").objectReferenceValue = meshTransform.transform;
        paperSO.FindProperty("paperFrontMaterial").objectReferenceValue = frontMat;
        paperSO.FindProperty("paperBackMaterial").objectReferenceValue = backMat;
        paperSO.FindProperty("paperFrontColor").colorValue = new Color(0.95f, 0.9f, 0.8f);
        paperSO.FindProperty("paperBackColor").colorValue = new Color(0.8f, 0.75f, 0.65f);
        paperSO.FindProperty("paperSize").floatValue = 5f;
        paperSO.FindProperty("tokenizeThreshold").floatValue = 0.1f;
        paperSO.ApplyModifiedProperties();

        MarkManager markManager = Object.FindFirstObjectByType<MarkManager>();
        if (markManager != null)
        {
            SerializedObject markManagerSO = new SerializedObject(markManager);
            markManagerSO.FindProperty("markParentTransform").objectReferenceValue = markTransform.transform;
            markManagerSO.ApplyModifiedProperties();
        }

        Debug.Log("[Foldlands] 종이 오브젝트 생성 완료");
    }

    [MenuItem(MENU_PATH + "7. UI 캔버스 생성")]
    public static void CreateUI()
    {
        DestroyExistingManager<UIManager>();

        GameObject canvasObj = new GameObject("FoldlandsCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        UIManager uiManager = canvasObj.AddComponent<UIManager>();

        // ========== 좌측 상단: 생존 정보 ==========
        GameObject survivalPanel = CreatePanel("SurvivalPanel", canvasObj.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10), new Vector2(200, 180));
        SurvivalUI survivalUI = survivalPanel.AddComponent<SurvivalUI>();

        GameObject dayTextObj = CreateTextMeshPro("DayText", survivalPanel.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10), new Vector2(180, 30), "Day 1", 24);

        GameObject foldTextObj = CreateTextMeshPro("FoldCountText", survivalPanel.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -40), new Vector2(180, 25), "Fold: 0/3", 18);

        GameObject totalPopTextObj = CreateTextMeshPro("TotalPopulationText", survivalPanel.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -70), new Vector2(180, 25), "Total Pop: 10", 18);

        GameObject activePopTextObj = CreateTextMeshPro("ActivePopulationText", survivalPanel.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -100), new Vector2(180, 25), "Active Pop: 0", 18);

        GameObject foodTextObj = CreateTextMeshPro("FoodText", survivalPanel.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -130), new Vector2(180, 25), "Food: 30", 18);

        GameObject areaTextObj = CreateTextMeshPro("PaperAreaText", survivalPanel.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -160), new Vector2(180, 25), "Paper: 100%", 18);

        SerializedObject survivalSO = new SerializedObject(survivalUI);
        survivalSO.FindProperty("dayText").objectReferenceValue = dayTextObj.GetComponent<TextMeshProUGUI>();
        survivalSO.FindProperty("foldCountText").objectReferenceValue = foldTextObj.GetComponent<TextMeshProUGUI>();
        survivalSO.FindProperty("totalPopulationText").objectReferenceValue = totalPopTextObj.GetComponent<TextMeshProUGUI>();
        survivalSO.FindProperty("activePopulationText").objectReferenceValue = activePopTextObj.GetComponent<TextMeshProUGUI>();
        survivalSO.FindProperty("foodText").objectReferenceValue = foodTextObj.GetComponent<TextMeshProUGUI>();
        survivalSO.FindProperty("paperAreaText").objectReferenceValue = areaTextObj.GetComponent<TextMeshProUGUI>();
        survivalSO.ApplyModifiedProperties();

        // ========== 우측 상단: 인벤토리 ==========
        GameObject inventoryPanel = CreatePanel("InventoryPanel", canvasObj.transform,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -10), new Vector2(200, 150));
        InventoryUI inventoryUI = inventoryPanel.AddComponent<InventoryUI>();

        GameObject invTextObj = CreateTextMeshPro("InventoryText", inventoryPanel.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10), new Vector2(180, 130), "<b>인벤토리</b>\n(비어있음)", 16);

        SerializedObject invSO = new SerializedObject(inventoryUI);
        invSO.FindProperty("inventoryText").objectReferenceValue = invTextObj.GetComponent<TextMeshProUGUI>();
        invSO.ApplyModifiedProperties();

        // ========== 중앙 하단: 예상 획득 자원 ==========
        GameObject expectPanel = CreatePanel("ExpectPanel", canvasObj.transform,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(250, 120));
        ExpectUI expectUI = expectPanel.AddComponent<ExpectUI>();

        GameObject expectTextObj = CreateTextMeshPro("ExpectText", expectPanel.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10), new Vector2(230, 100), "<b>예상 획득 자원</b>\n(없음)", 16);

        SerializedObject expectSO = new SerializedObject(expectUI);
        expectSO.FindProperty("expectText").objectReferenceValue = expectTextObj.GetComponent<TextMeshProUGUI>();
        expectSO.FindProperty("expectPanel").objectReferenceValue = expectPanel;
        expectSO.ApplyModifiedProperties();

        expectPanel.SetActive(false);

        // ========== 중앙: 인구 선택 UI ==========
        GameObject popSelectPanel = CreatePanel("PopulationSelectPanel", canvasObj.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 200));
        popSelectPanel.GetComponent<UnityEngine.UI.Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        PopulationSelectUI popSelectUI = popSelectPanel.AddComponent<PopulationSelectUI>();

        GameObject availablePopTextObj = CreateTextMeshPro("AvailablePopulationText", popSelectPanel.transform,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(260, 30), "사용 가능 인구: 10명", 18);
        availablePopTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // 버튼들 (-, 숫자, +)
        GameObject buttonRow = new GameObject("ButtonRow");
        buttonRow.transform.SetParent(popSelectPanel.transform);
        RectTransform buttonRowRect = buttonRow.AddComponent<RectTransform>();
        buttonRowRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRowRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRowRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRowRect.anchoredPosition = Vector2.zero;
        buttonRowRect.sizeDelta = new Vector2(200, 50);

        GameObject decreaseBtn = CreateButton("DecreaseButton", buttonRow.transform, new Vector2(-80, 0), new Vector2(50, 50), "-", 24);
        GameObject countTextObj = CreateTextMeshPro("SelectedCountText", buttonRow.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 50), "1", 32);
        countTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        GameObject increaseBtn = CreateButton("IncreaseButton", buttonRow.transform, new Vector2(80, 0), new Vector2(50, 50), "+", 24);

        GameObject confirmBtn = CreateButton("ConfirmButton", popSelectPanel.transform, new Vector2(0, -60), new Vector2(120, 40), "확정", 18);

        SerializedObject popSelectSO = new SerializedObject(popSelectUI);
        popSelectSO.FindProperty("panel").objectReferenceValue = popSelectPanel;
        popSelectSO.FindProperty("availablePopulationText").objectReferenceValue = availablePopTextObj.GetComponent<TextMeshProUGUI>();
        popSelectSO.FindProperty("selectedCountText").objectReferenceValue = countTextObj.GetComponent<TextMeshProUGUI>();
        popSelectSO.FindProperty("decreaseButton").objectReferenceValue = decreaseBtn.GetComponent<Button>();
        popSelectSO.FindProperty("increaseButton").objectReferenceValue = increaseBtn.GetComponent<Button>();
        popSelectSO.FindProperty("confirmButton").objectReferenceValue = confirmBtn.GetComponent<Button>();
        popSelectSO.ApplyModifiedProperties();

        // ========== 게임오버 패널 ==========
        GameObject gameOverPanel = CreatePanel("GameOverPanel", canvasObj.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 200));
        gameOverPanel.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.8f);
        gameOverPanel.SetActive(false);

        GameObject gameOverTextObj = CreateTextMeshPro("GameOverText", gameOverPanel.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 180), "게임 오버\n\n생존 일수: 0일", 28);
        gameOverTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        SerializedObject uiSO = new SerializedObject(uiManager);
        uiSO.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        uiSO.FindProperty("gameOverText").objectReferenceValue = gameOverTextObj.GetComponent<TextMeshProUGUI>();
        uiSO.ApplyModifiedProperties();

        Debug.Log("[Foldlands] UI 캔버스 생성 완료");
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        UnityEngine.UI.Image image = panel.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0, 0, 0, 0.5f);

        return panel;
    }

    private static GameObject CreateTextMeshPro(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, string text, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft;

        return textObj;
    }

    private static GameObject CreateButton(string name, Transform parent, Vector2 anchoredPos, Vector2 size, string text, int fontSize)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        UnityEngine.UI.Image image = btnObj.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        Button button = btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btnObj;
    }

    private static void DestroyExistingManager<T>() where T : MonoBehaviour
    {
        T existing = Object.FindFirstObjectByType<T>();
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }
    }

    [MenuItem(MENU_PATH + "카메라 설정")]
    public static void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }

        mainCam.orthographic = true;
        mainCam.orthographicSize = 5f;
        mainCam.transform.position = new Vector3(0, 0, -10);
        mainCam.backgroundColor = new Color(0.2f, 0.2f, 0.3f);

        Debug.Log("[Foldlands] 카메라 설정 완료");
    }
}