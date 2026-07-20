using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UIPrefabBuilderUtils;

/// <summary>
/// Editor-only tool that builds the player HUD's inventory grid (a fixed
/// 3x2 grid of item-icon slots anchored to the bottom-right of the HUD)
/// into UI.prefab, and creates the InventorySlot.prefab it needs.
/// Safe to re-run.
/// </summary>
public static class PlayerInventoryUIBuilder
{
    private const string UIPrefabPath = "Assets/Prefabs/UI.prefab";
    private const string InventorySlotPrefabPath = "Assets/Prefabs/InventorySlot.prefab";
    private const string StatLinePrefabPath = "Assets/Prefabs/ShopStatLine.prefab";
    private const float SlotSize = 48f;
    private const float SlotSpacing = 6f;
    private const int InventoryColumns = 3;
    private const int InventoryRows = 2;
    private const float InventoryRightOffset = 300f;
    private const float InventoryBottomOffset = 16f;
    private const float TooltipWidth = 180f;

    [MenuItem("Tools/Player/Build Inventory UI")]
    public static void Build()
    {
        BuildInventorySlotPrefab();
        BuildInventoryRow();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[PlayerInventoryUIBuilder] Player inventory UI built.");
    }

    // ---------------------------------------------------------------
    // InventorySlot.prefab — a single icon square; empty until an
    // owned item is assigned to it.
    // ---------------------------------------------------------------
    private static void BuildInventorySlotPrefab()
    {
        GameObject root = LoadOrCreatePrefabRoot(InventorySlotPrefabPath, "InventorySlot", out bool isNew);

        RectTransform rootRt = GetOrAdd<RectTransform>(root);
        rootRt.sizeDelta = new Vector2(SlotSize, SlotSize);
        LayoutElement rootLe = GetOrAdd<LayoutElement>(root);
        rootLe.preferredWidth = SlotSize;
        rootLe.preferredHeight = SlotSize;

        Image background = GetOrAdd<Image>(root);
        background.color = new Color(0f, 0f, 0f, 0.4f);

        GameObject iconGo = FindOrCreateChild(root.transform, "Icon");
        RectTransform iconRt = GetOrAdd<RectTransform>(iconGo);
        StretchFill(iconRt);
        Image iconImg = GetOrAdd<Image>(iconGo);
        iconImg.color = Color.white;
        iconImg.preserveAspect = true;
        iconImg.enabled = false;

        InventorySlotUI slotUi = GetOrAdd<InventorySlotUI>(root);
        AssignSerialized(slotUi, "icon", iconImg);

        SavePrefabRoot(root, InventorySlotPrefabPath, isNew);
    }

    // ---------------------------------------------------------------
    // UI.prefab — InventoryRow: a 3-column x 2-row grid of item slots,
    // anchored to the bottom-right corner of the HUD (same parent as
    // the existing Abilities row, found by name rather than assumed),
    // offset InventoryRightOffset px in from the right edge.
    // ---------------------------------------------------------------
    private static void BuildInventoryRow()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(UIPrefabPath) == null)
        {
            Debug.LogError($"[PlayerInventoryUIBuilder] {UIPrefabPath} not found.");
            return;
        }

        GameObject uiRoot = PrefabUtility.LoadPrefabContents(UIPrefabPath);

        Transform abilities = FindRecursive(uiRoot.transform, "Abilities");
        if (abilities == null)
        {
            Debug.LogError("[PlayerInventoryUIBuilder] Could not find 'Abilities' inside UI.prefab.");
            PrefabUtility.UnloadPrefabContents(uiRoot);
            return;
        }

        Transform parent = abilities.parent != null ? abilities.parent : uiRoot.transform;

        GameObject inventoryRow = FindOrCreateChild(parent, "InventoryRow");

        // Remove a stale single-row layout from an earlier build, if present,
        // so it doesn't fight the GridLayoutGroup for control.
        HorizontalLayoutGroup staleRowLayout = inventoryRow.GetComponent<HorizontalLayoutGroup>();
        if (staleRowLayout != null) Object.DestroyImmediate(staleRowLayout);

        float gridWidth = InventoryColumns * SlotSize + (InventoryColumns - 1) * SlotSpacing;
        float gridHeight = InventoryRows * SlotSize + (InventoryRows - 1) * SlotSpacing;

        RectTransform rowRt = GetOrAdd<RectTransform>(inventoryRow);
        rowRt.anchorMin = new Vector2(1, 0);
        rowRt.anchorMax = new Vector2(1, 0);
        rowRt.pivot = new Vector2(1, 0);
        rowRt.sizeDelta = new Vector2(gridWidth, gridHeight);
        rowRt.anchoredPosition = new Vector2(-InventoryRightOffset, InventoryBottomOffset);

        GridLayoutGroup grid = GetOrAdd<GridLayoutGroup>(inventoryRow);
        grid.cellSize = new Vector2(SlotSize, SlotSize);
        grid.spacing = new Vector2(SlotSpacing, SlotSpacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = InventoryColumns;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.childAlignment = TextAnchor.MiddleCenter;

        LayoutElement rowLe = GetOrAdd<LayoutElement>(inventoryRow);
        rowLe.preferredWidth = gridWidth;
        rowLe.preferredHeight = gridHeight;

        PlayerHUD hud = uiRoot.GetComponentInChildren<PlayerHUD>(true);
        if (hud == null)
        {
            Debug.LogError("[PlayerInventoryUIBuilder] Could not find a PlayerHUD component in UI.prefab.");
        }
        else
        {
            GameObject slotAsset = AssetDatabase.LoadAssetAtPath<GameObject>(InventorySlotPrefabPath);
            AssignSerialized(hud, "inventoryContainer", inventoryRow.transform);
            AssignSerialized(hud, "inventorySlotPrefab", slotAsset);
            BuildTooltip(parent, hud);
        }

        PrefabUtility.SaveAsPrefabAsset(uiRoot, UIPrefabPath);
        PrefabUtility.UnloadPrefabContents(uiRoot);
    }

    // ---------------------------------------------------------------
    // InventoryTooltip: a floating panel shown on hover. The
    // InventoryTooltipUI component lives on an always-active wrapper;
    // only its "Panel" child is toggled, so the component's own Awake
    // isn't interrupted by the very SetActive(true) call that reveals it.
    // ---------------------------------------------------------------
    private static void BuildTooltip(Transform parent, PlayerHUD hud)
    {
        GameObject tooltipRoot = FindOrCreateChild(parent, "InventoryTooltip");
        tooltipRoot.transform.SetAsLastSibling();
        RectTransform tooltipRootRt = GetOrAdd<RectTransform>(tooltipRoot);
        StretchFill(tooltipRootRt);

        GameObject panel = FindOrCreateChild(tooltipRoot.transform, "Panel");
        RectTransform panelRt = GetOrAdd<RectTransform>(panel);
        panelRt.anchorMin = new Vector2(0.5f, 0f);
        panelRt.anchorMax = new Vector2(0.5f, 0f);
        panelRt.pivot = new Vector2(0.5f, 0f);
        panelRt.sizeDelta = new Vector2(TooltipWidth, 0);

        Image bg = GetOrAdd<Image>(panel);
        bg.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);
        bg.raycastTarget = false;

        VerticalLayoutGroup layout = GetOrAdd<VerticalLayoutGroup>(panel);
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.spacing = 4;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        ContentSizeFitter fitter = GetOrAdd<ContentSizeFitter>(panel);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject nameGo = FindOrCreateChild(panel.transform, "NameText");
        TextMeshProUGUI nameTmp = GetOrAdd<TextMeshProUGUI>(nameGo);
        nameTmp.text = "Item Name";
        nameTmp.fontSize = 16;
        ApplyMedievalFont(nameTmp);

        GameObject statsGo = FindOrCreateChild(panel.transform, "StatsStack");
        VerticalLayoutGroup statsLayout = GetOrAdd<VerticalLayoutGroup>(statsGo);
        statsLayout.spacing = 2;
        statsLayout.childForceExpandWidth = true;
        statsLayout.childForceExpandHeight = false;
        statsLayout.childControlWidth = true;
        statsLayout.childControlHeight = true;

        GameObject costGo = FindOrCreateChild(panel.transform, "CostText");
        TextMeshProUGUI costTmp = GetOrAdd<TextMeshProUGUI>(costGo);
        costTmp.text = "0g";
        costTmp.fontSize = 13;
        ApplyMedievalFont(costTmp);

        GameObject sellGo = FindOrCreateChild(panel.transform, "SellText");
        TextMeshProUGUI sellTmp = GetOrAdd<TextMeshProUGUI>(sellGo);
        sellTmp.text = "Sell: 70% (0g)";
        sellTmp.fontSize = 12;
        sellTmp.color = new Color(0.6f, 0.55f, 0.4f, 1f);
        ApplyMedievalFont(sellTmp);

        panel.SetActive(false);

        InventoryTooltipUI tooltipUi = GetOrAdd<InventoryTooltipUI>(tooltipRoot);
        AssignSerialized(tooltipUi, "panel", panelRt);
        AssignSerialized(tooltipUi, "nameText", nameTmp);
        AssignSerialized(tooltipUi, "costText", costTmp);
        AssignSerialized(tooltipUi, "sellText", sellTmp);
        AssignSerialized(tooltipUi, "statsContainer", statsGo.transform);
        GameObject statLineAsset = AssetDatabase.LoadAssetAtPath<GameObject>(StatLinePrefabPath);
        AssignSerialized(tooltipUi, "statLinePrefab", statLineAsset);

        AssignSerialized(hud, "inventoryTooltip", tooltipUi);
    }
}
