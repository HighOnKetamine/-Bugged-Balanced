using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor-only tool that programmatically builds the two-column Shop UI
/// hierarchy (filter bar, scrollable item grid, divider, details panel)
/// into UI.prefab and ItemSlot.prefab, and creates the small supporting
/// prefabs it needs. Safe to re-run (idempotent — reuses existing
/// GameObjects/components by name instead of duplicating them).
/// </summary>
public static class ShopUIBuilder
{
    private const string UIPrefabPath = "Assets/Prefabs/UI.prefab";
    private const string ItemSlotPrefabPath = "Assets/Prefabs/ItemSlot.prefab";
    private const string FilterButtonPrefabPath = "Assets/Prefabs/ShopFilterButton.prefab";
    private const string StatLinePrefabPath = "Assets/Prefabs/ShopStatLine.prefab";

    // Safe-area margins so content clears the painted border of whatever
    // background art is assigned to ShopPanel (frame art typically has
    // transparent padding around the visible book/parchment shape, so
    // content can't safely sit flush against the RectTransform's edges).
    // Tune these if the assigned art still clips — bigger number = more
    // inward clearance from that edge.
    private const float PanelPaddingTop = 64f;
    private const float PanelPaddingSides = 48f;
    private const float PanelPaddingBottom = 56f;
    private const float HeaderHeight = 48f;
    private const float HeaderBodyGap = 16f;

    [MenuItem("Tools/Shop/Build Shop UI")]
    public static void Build()
    {
        BuildItemSlotPrefab();
        BuildFilterButtonPrefab();
        BuildStatLinePrefab();
        BuildShopPanel();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ShopUIBuilder] Shop UI hierarchy built.");
    }

    // ---------------------------------------------------------------
    // ItemSlot.prefab — grid cell: icon, name, cost, selection highlight,
    // whole-cell click target. Old inline Description/BuyButton removed.
    // ---------------------------------------------------------------
    private static void BuildItemSlotPrefab()
    {
        GameObject root = UIPrefabBuilderUtils.LoadOrCreatePrefabRoot(ItemSlotPrefabPath, "ItemSlot", out bool isNew);

        RectTransform rootRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(root);
        rootRt.sizeDelta = new Vector2(110, 130);

        UIPrefabBuilderUtils.RemoveChild(root.transform, "Description");
        UIPrefabBuilderUtils.RemoveChild(root.transform, "BuyButton");

        VerticalLayoutGroup vlg = UIPrefabBuilderUtils.GetOrAdd<VerticalLayoutGroup>(root);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.spacing = 4;
        vlg.padding = new RectOffset(4, 4, 4, 4);

        Image rootImage = UIPrefabBuilderUtils.GetOrAdd<Image>(root);
        rootImage.color = new Color(1f, 1f, 1f, 0.05f);
        Button selectButton = UIPrefabBuilderUtils.GetOrAdd<Button>(root);
        selectButton.targetGraphic = rootImage;

        GameObject highlight = UIPrefabBuilderUtils.FindOrCreateChild(root.transform, "SelectedHighlight");
        RectTransform highlightRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(highlight);
        UIPrefabBuilderUtils.StretchFill(highlightRt);
        LayoutElement highlightLe = UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(highlight);
        highlightLe.ignoreLayout = true;
        Image highlightImg = UIPrefabBuilderUtils.GetOrAdd<Image>(highlight);
        highlightImg.color = new Color(1f, 0.85f, 0.2f, 0.35f);
        highlightImg.raycastTarget = false;
        highlight.transform.SetAsFirstSibling();
        highlight.SetActive(false);

        GameObject icon = UIPrefabBuilderUtils.FindOrCreateChild(root.transform, "Icon");
        Image iconImg = UIPrefabBuilderUtils.GetOrAdd<Image>(icon);
        iconImg.color = Color.gray;
        LayoutElement iconLe = UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(icon);
        iconLe.preferredWidth = 64;
        iconLe.preferredHeight = 64;

        GameObject nameGo = UIPrefabBuilderUtils.FindOrCreateChild(root.transform, "Name");
        TextMeshProUGUI nameTmp = UIPrefabBuilderUtils.GetOrAdd<TextMeshProUGUI>(nameGo);
        nameTmp.text = "Item Name";
        nameTmp.fontSize = 14;
        nameTmp.alignment = TextAlignmentOptions.Center;
        nameTmp.enableWordWrapping = true;
        UIPrefabBuilderUtils.ApplyMedievalFont(nameTmp);
        UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(nameGo).preferredHeight = 34;

        GameObject costGo = UIPrefabBuilderUtils.FindOrCreateChild(root.transform, "Cost");
        TextMeshProUGUI costTmp = UIPrefabBuilderUtils.GetOrAdd<TextMeshProUGUI>(costGo);
        costTmp.text = "0g";
        costTmp.fontSize = 12;
        costTmp.alignment = TextAlignmentOptions.Center;
        UIPrefabBuilderUtils.ApplyMedievalFont(costTmp);
        UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(costGo).preferredHeight = 18;

        ShopItemSlotUI slotUi = UIPrefabBuilderUtils.GetOrAdd<ShopItemSlotUI>(root);
        UIPrefabBuilderUtils.AssignSerialized(slotUi, "icon", iconImg);
        UIPrefabBuilderUtils.AssignSerialized(slotUi, "nameText", nameTmp);
        UIPrefabBuilderUtils.AssignSerialized(slotUi, "costText", costTmp);
        UIPrefabBuilderUtils.AssignSerialized(slotUi, "selectedHighlight", highlight);
        UIPrefabBuilderUtils.AssignSerialized(slotUi, "selectButton", selectButton);

        UIPrefabBuilderUtils.SavePrefabRoot(root, ItemSlotPrefabPath, isNew);
    }

    // ---------------------------------------------------------------
    // ShopFilterButton.prefab — one category tab.
    // ---------------------------------------------------------------
    private static void BuildFilterButtonPrefab()
    {
        GameObject root = UIPrefabBuilderUtils.LoadOrCreatePrefabRoot(FilterButtonPrefabPath, "ShopFilterButton", out bool isNew);

        RectTransform rootRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(root);
        rootRt.sizeDelta = new Vector2(70, 32);
        LayoutElement rootLe = UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(root);
        rootLe.minWidth = 40;
        rootLe.preferredWidth = 70;
        rootLe.flexibleWidth = 1;
        rootLe.minHeight = 32;
        rootLe.preferredHeight = 32;
        rootLe.flexibleHeight = 0;

        Image bg = UIPrefabBuilderUtils.GetOrAdd<Image>(root);
        bg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        Button button = UIPrefabBuilderUtils.GetOrAdd<Button>(root);
        button.targetGraphic = bg;

        GameObject highlight = UIPrefabBuilderUtils.FindOrCreateChild(root.transform, "SelectedHighlight");
        RectTransform highlightRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(highlight);
        UIPrefabBuilderUtils.StretchFill(highlightRt);
        UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(highlight).ignoreLayout = true;
        Image highlightImg = UIPrefabBuilderUtils.GetOrAdd<Image>(highlight);
        highlightImg.color = new Color(1f, 0.85f, 0.2f, 0.5f);
        highlightImg.raycastTarget = false;
        highlight.SetActive(false);

        GameObject labelGo = UIPrefabBuilderUtils.FindOrCreateChild(root.transform, "Label");
        RectTransform labelRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(labelGo);
        UIPrefabBuilderUtils.StretchFill(labelRt);
        UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(labelGo).ignoreLayout = true;
        TextMeshProUGUI label = UIPrefabBuilderUtils.GetOrAdd<TextMeshProUGUI>(labelGo);
        label.text = "Category";
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 8;
        label.fontSizeMax = 14;
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Truncate;
        UIPrefabBuilderUtils.ApplyMedievalFont(label);

        ShopFilterButtonUI filterUi = UIPrefabBuilderUtils.GetOrAdd<ShopFilterButtonUI>(root);
        UIPrefabBuilderUtils.AssignSerialized(filterUi, "label", label);
        UIPrefabBuilderUtils.AssignSerialized(filterUi, "button", button);
        UIPrefabBuilderUtils.AssignSerialized(filterUi, "selectedHighlight", highlight);

        UIPrefabBuilderUtils.SavePrefabRoot(root, FilterButtonPrefabPath, isNew);
    }

    // ---------------------------------------------------------------
    // ShopStatLine.prefab — bare text row instantiated per ItemModifier.
    // ---------------------------------------------------------------
    private static void BuildStatLinePrefab()
    {
        GameObject root = UIPrefabBuilderUtils.LoadOrCreatePrefabRoot(StatLinePrefabPath, "ShopStatLine", out bool isNew);

        UIPrefabBuilderUtils.GetOrAdd<RectTransform>(root).sizeDelta = new Vector2(160, 20);
        UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(root).preferredHeight = 20;

        TextMeshProUGUI text = UIPrefabBuilderUtils.GetOrAdd<TextMeshProUGUI>(root);
        text.text = "+0 Stat";
        text.fontSize = 14;
        text.alignment = TextAlignmentOptions.Left;
        UIPrefabBuilderUtils.ApplyMedievalFont(text);

        UIPrefabBuilderUtils.SavePrefabRoot(root, StatLinePrefabPath, isNew);
    }

    // ---------------------------------------------------------------
    // UI.prefab — ShopPanel: Header(+Close), Body(Left/Divider/Right).
    // ---------------------------------------------------------------
    private static void BuildShopPanel()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(UIPrefabPath) == null)
        {
            Debug.LogError($"[ShopUIBuilder] {UIPrefabPath} not found.");
            return;
        }

        GameObject uiRoot = PrefabUtility.LoadPrefabContents(UIPrefabPath);

        Transform shopPanel = UIPrefabBuilderUtils.FindRecursive(uiRoot.transform, "ShopPanel");
        if (shopPanel == null)
        {
            Debug.LogError("[ShopUIBuilder] Could not find 'ShopPanel' inside UI.prefab.");
            PrefabUtility.UnloadPrefabContents(uiRoot);
            return;
        }

        RectTransform panelRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(shopPanel.gameObject);
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(900, 600);
        UIPrefabBuilderUtils.GetOrAdd<Image>(shopPanel.gameObject);

        // ---- Header ----
        GameObject header = UIPrefabBuilderUtils.FindOrCreateChild(shopPanel, "Header");
        RectTransform headerRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(header);
        headerRt.anchorMin = new Vector2(0, 1);
        headerRt.anchorMax = new Vector2(1, 1);
        headerRt.pivot = new Vector2(0.5f, 1f);
        headerRt.sizeDelta = new Vector2(-(PanelPaddingSides * 2f), HeaderHeight);
        headerRt.anchoredPosition = new Vector2(0, -PanelPaddingTop);

        GameObject title = UIPrefabBuilderUtils.FindOrCreateChild(header.transform, "Title");
        RectTransform titleRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(title);
        titleRt.anchorMin = new Vector2(0, 0);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.offsetMin = new Vector2(16, 0);
        titleRt.offsetMax = new Vector2(-56, 0);
        TextMeshProUGUI titleTmp = UIPrefabBuilderUtils.GetOrAdd<TextMeshProUGUI>(title);
        titleTmp.text = "SHOP";
        titleTmp.fontSize = 28;
        titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
        UIPrefabBuilderUtils.ApplyMedievalFont(titleTmp);

        GameObject closeBtnGo = UIPrefabBuilderUtils.FindOrCreateChild(header.transform, "CloseButton");
        RectTransform closeBtnRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(closeBtnGo);
        closeBtnRt.anchorMin = new Vector2(1, 0.5f);
        closeBtnRt.anchorMax = new Vector2(1, 0.5f);
        closeBtnRt.pivot = new Vector2(1, 0.5f);
        closeBtnRt.anchoredPosition = new Vector2(-12, 0);
        closeBtnRt.sizeDelta = new Vector2(36, 36);
        Image closeBg = UIPrefabBuilderUtils.GetOrAdd<Image>(closeBtnGo);
        closeBg.color = new Color(0.3f, 0.1f, 0.1f, 1f);
        Button closeButton = UIPrefabBuilderUtils.GetOrAdd<Button>(closeBtnGo);
        closeButton.targetGraphic = closeBg;

        GameObject closeLabelGo = UIPrefabBuilderUtils.FindOrCreateChild(closeBtnGo.transform, "Label");
        RectTransform closeLabelRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(closeLabelGo);
        UIPrefabBuilderUtils.StretchFill(closeLabelRt);
        TextMeshProUGUI closeLabel = UIPrefabBuilderUtils.GetOrAdd<TextMeshProUGUI>(closeLabelGo);
        closeLabel.text = "X";
        closeLabel.alignment = TextAlignmentOptions.Center;
        closeLabel.fontSize = 20;
        UIPrefabBuilderUtils.ApplyMedievalFont(closeLabel);

        // ---- Gold text (pre-existing element) — reposition into the header,
        // clear of the close button, instead of wherever it used to sit ----
        Transform goldTextTransform = UIPrefabBuilderUtils.FindRecursive(shopPanel, "GoldText");
        if (goldTextTransform != null)
        {
            goldTextTransform.SetParent(header.transform, false);
            RectTransform goldRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(goldTextTransform.gameObject);
            goldRt.anchorMin = new Vector2(1, 0);
            goldRt.anchorMax = new Vector2(1, 1);
            goldRt.pivot = new Vector2(1, 0.5f);
            goldRt.sizeDelta = new Vector2(160, 0);
            goldRt.anchoredPosition = new Vector2(-(36 + 12 + 12), 0);

            TextMeshProUGUI goldTmp = goldTextTransform.GetComponent<TextMeshProUGUI>();
            if (goldTmp != null)
            {
                goldTmp.alignment = TextAlignmentOptions.MidlineRight;
                goldTmp.fontSize = 18;
                UIPrefabBuilderUtils.ApplyMedievalFont(goldTmp);
            }
        }

        // ---- Body ----
        GameObject body = UIPrefabBuilderUtils.FindOrCreateChild(shopPanel, "Body");
        RectTransform bodyRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(body);
        bodyRt.anchorMin = new Vector2(0, 0);
        bodyRt.anchorMax = new Vector2(1, 1);
        bodyRt.offsetMin = new Vector2(PanelPaddingSides, PanelPaddingBottom);
        bodyRt.offsetMax = new Vector2(-PanelPaddingSides, -(PanelPaddingTop + HeaderHeight + HeaderBodyGap));
        HorizontalLayoutGroup bodyLayout = UIPrefabBuilderUtils.GetOrAdd<HorizontalLayoutGroup>(body);
        bodyLayout.spacing = 16;
        bodyLayout.childForceExpandWidth = false;
        bodyLayout.childForceExpandHeight = true;
        bodyLayout.childControlWidth = true;
        bodyLayout.childControlHeight = true;

        // ---- Left column: filter bar + scrollable grid ----
        GameObject leftColumn = UIPrefabBuilderUtils.FindOrCreateChild(body.transform, "LeftColumn");
        LayoutElement leftColumnLe = UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(leftColumn);
        leftColumnLe.minWidth = 0;
        leftColumnLe.preferredWidth = 0;
        leftColumnLe.flexibleWidth = 1;
        VerticalLayoutGroup leftLayout = UIPrefabBuilderUtils.GetOrAdd<VerticalLayoutGroup>(leftColumn);
        leftLayout.spacing = 12;
        leftLayout.childForceExpandWidth = true;
        leftLayout.childForceExpandHeight = false;
        leftLayout.childControlWidth = true;
        leftLayout.childControlHeight = true;

        GameObject filterBar = UIPrefabBuilderUtils.FindOrCreateChild(leftColumn.transform, "FilterBar");
        LayoutElement filterBarLe = UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(filterBar);
        filterBarLe.minHeight = 36;
        filterBarLe.preferredHeight = 36;
        filterBarLe.flexibleHeight = 0;
        HorizontalLayoutGroup filterBarLayout = UIPrefabBuilderUtils.GetOrAdd<HorizontalLayoutGroup>(filterBar);
        filterBarLayout.spacing = 6;
        filterBarLayout.childForceExpandWidth = true;
        filterBarLayout.childForceExpandHeight = false;
        filterBarLayout.childControlWidth = true;
        filterBarLayout.childControlHeight = true;
        filterBarLayout.childAlignment = TextAnchor.MiddleLeft;

        GameObject scrollGo = UIPrefabBuilderUtils.FindOrCreateChild(leftColumn.transform, "GridScrollView");
        LayoutElement scrollLe = UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(scrollGo);
        scrollLe.minHeight = 0;
        scrollLe.preferredHeight = 0;
        scrollLe.flexibleHeight = 1;
        Image scrollBg = UIPrefabBuilderUtils.GetOrAdd<Image>(scrollGo);
        scrollBg.color = new Color(0, 0, 0, 0.15f);
        ScrollRect scrollRect = UIPrefabBuilderUtils.GetOrAdd<ScrollRect>(scrollGo);
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        GameObject viewport = UIPrefabBuilderUtils.FindOrCreateChild(scrollGo.transform, "Viewport");
        RectTransform viewportRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(viewport);
        UIPrefabBuilderUtils.StretchFill(viewportRt);
        UIPrefabBuilderUtils.GetOrAdd<RectMask2D>(viewport);
        Image viewportImg = UIPrefabBuilderUtils.GetOrAdd<Image>(viewport);
        viewportImg.color = new Color(0, 0, 0, 0);

        GameObject content = UIPrefabBuilderUtils.FindOrCreateChild(viewport.transform, "Content");
        RectTransform contentRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(content);
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;

        GridLayoutGroup grid = UIPrefabBuilderUtils.GetOrAdd<GridLayoutGroup>(content);
        grid.cellSize = new Vector2(110, 130);
        grid.spacing = new Vector2(10, 10);
        grid.padding = new RectOffset(8, 8, 8, 8);
        grid.constraint = GridLayoutGroup.Constraint.Flexible;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;

        ContentSizeFitter fitter = UIPrefabBuilderUtils.GetOrAdd<ContentSizeFitter>(content);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRt;
        scrollRect.viewport = viewportRt;

        // ---- Divider ----
        GameObject divider = UIPrefabBuilderUtils.FindOrCreateChild(body.transform, "Divider");
        LayoutElement dividerLe = UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(divider);
        dividerLe.preferredWidth = 3;
        dividerLe.flexibleWidth = 0;
        Image dividerImg = UIPrefabBuilderUtils.GetOrAdd<Image>(divider);
        dividerImg.color = new Color(1f, 1f, 1f, 0.25f);
        dividerImg.raycastTarget = false;

        // ---- Right column: details panel ----
        GameObject rightColumn = UIPrefabBuilderUtils.FindOrCreateChild(body.transform, "RightColumn");
        LayoutElement rightColumnLe = UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(rightColumn);
        rightColumnLe.minWidth = 0;
        rightColumnLe.preferredWidth = 0;
        rightColumnLe.flexibleWidth = 1;
        BuildDetailsPanel(rightColumn, out ShopDetailsPanelUI detailsPanelUi);

        // ---- Wire ShopUI ----
        ShopUI shopUi = uiRoot.GetComponentInChildren<ShopUI>(true);
        if (shopUi == null)
        {
            Debug.LogError("[ShopUIBuilder] Could not find a ShopUI component in UI.prefab.");
        }
        else
        {
            GameObject filterButtonAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FilterButtonPrefabPath);
            GameObject itemSlotAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ItemSlotPrefabPath);

            UIPrefabBuilderUtils.AssignSerialized(shopUi, "shopPanel", shopPanel.gameObject);
            UIPrefabBuilderUtils.AssignSerialized(shopUi, "closeButton", closeButton);
            UIPrefabBuilderUtils.AssignSerialized(shopUi, "filterBarContainer", filterBar.transform);
            UIPrefabBuilderUtils.AssignSerialized(shopUi, "filterButtonPrefab", filterButtonAsset);
            UIPrefabBuilderUtils.AssignSerialized(shopUi, "itemsContainer", content.transform);
            UIPrefabBuilderUtils.AssignSerialized(shopUi, "itemSlotPrefab", itemSlotAsset);
            UIPrefabBuilderUtils.AssignSerialized(shopUi, "detailsPanel", detailsPanelUi);
        }

        PrefabUtility.SaveAsPrefabAsset(uiRoot, UIPrefabPath);
        PrefabUtility.UnloadPrefabContents(uiRoot);
    }

    private static void BuildDetailsPanel(GameObject rightColumn, out ShopDetailsPanelUI detailsPanelUi)
    {
        VerticalLayoutGroup rightLayout = UIPrefabBuilderUtils.GetOrAdd<VerticalLayoutGroup>(rightColumn);
        rightLayout.spacing = 12;
        rightLayout.childForceExpandWidth = true;
        rightLayout.childForceExpandHeight = false;
        rightLayout.childControlWidth = true;
        rightLayout.childControlHeight = true;
        rightLayout.padding = new RectOffset(12, 12, 12, 12);

        // Empty (default) state
        GameObject emptyState = UIPrefabBuilderUtils.FindOrCreateChild(rightColumn.transform, "EmptyState");
        UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(emptyState).flexibleHeight = 1;
        TextMeshProUGUI emptyText = UIPrefabBuilderUtils.GetOrAdd<TextMeshProUGUI>(emptyState);
        emptyText.text = "Select an item to view details";
        emptyText.alignment = TextAlignmentOptions.Center;
        emptyText.fontSize = 16;
        UIPrefabBuilderUtils.ApplyMedievalFont(emptyText);
        emptyState.SetActive(true);

        // Populated content root
        GameObject content = UIPrefabBuilderUtils.FindOrCreateChild(rightColumn.transform, "Content");
        VerticalLayoutGroup contentLayout = UIPrefabBuilderUtils.GetOrAdd<VerticalLayoutGroup>(content);
        contentLayout.spacing = 12;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        content.SetActive(false);

        GameObject nameHeaderGo = UIPrefabBuilderUtils.FindOrCreateChild(content.transform, "NameHeader");
        UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(nameHeaderGo).preferredHeight = 32;
        TextMeshProUGUI nameHeader = UIPrefabBuilderUtils.GetOrAdd<TextMeshProUGUI>(nameHeaderGo);
        nameHeader.text = "Item Name";
        nameHeader.fontSize = 24;
        nameHeader.alignment = TextAlignmentOptions.MidlineLeft;
        UIPrefabBuilderUtils.ApplyMedievalFont(nameHeader);

        GameObject splitRow = UIPrefabBuilderUtils.FindOrCreateChild(content.transform, "SplitRow");
        UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(splitRow).preferredHeight = 160;
        HorizontalLayoutGroup splitLayout = UIPrefabBuilderUtils.GetOrAdd<HorizontalLayoutGroup>(splitRow);
        splitLayout.spacing = 12;
        splitLayout.childForceExpandWidth = false;
        splitLayout.childForceExpandHeight = true;
        splitLayout.childControlWidth = true;
        splitLayout.childControlHeight = true;

        GameObject imageQuoteColumn = UIPrefabBuilderUtils.FindOrCreateChild(splitRow.transform, "ImageQuoteColumn");
        LayoutElement iqLe = UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(imageQuoteColumn);
        iqLe.preferredWidth = 160;
        iqLe.flexibleWidth = 0;
        VerticalLayoutGroup iqLayout = UIPrefabBuilderUtils.GetOrAdd<VerticalLayoutGroup>(imageQuoteColumn);
        iqLayout.spacing = 8;
        iqLayout.childForceExpandWidth = true;
        iqLayout.childForceExpandHeight = false;
        iqLayout.childControlWidth = true;
        iqLayout.childControlHeight = true;

        GameObject itemImageGo = UIPrefabBuilderUtils.FindOrCreateChild(imageQuoteColumn.transform, "ItemImage");
        LayoutElement itemImageLe = UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(itemImageGo);
        itemImageLe.preferredWidth = 140;
        itemImageLe.preferredHeight = 140;
        Image itemImage = UIPrefabBuilderUtils.GetOrAdd<Image>(itemImageGo);
        itemImage.color = Color.gray;

        GameObject flavorTextGo = UIPrefabBuilderUtils.FindOrCreateChild(imageQuoteColumn.transform, "FlavorText");
        UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(flavorTextGo).flexibleHeight = 1;
        TextMeshProUGUI flavorText = UIPrefabBuilderUtils.GetOrAdd<TextMeshProUGUI>(flavorTextGo);
        flavorText.text = "\"quote about the item\"";
        flavorText.fontStyle = FontStyles.Italic;
        flavorText.fontSize = 12;
        flavorText.enableWordWrapping = true;
        UIPrefabBuilderUtils.ApplyMedievalFont(flavorText);

        GameObject statsStack = UIPrefabBuilderUtils.FindOrCreateChild(splitRow.transform, "StatsStack");
        UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(statsStack).flexibleWidth = 1;
        VerticalLayoutGroup statsLayout = UIPrefabBuilderUtils.GetOrAdd<VerticalLayoutGroup>(statsStack);
        statsLayout.spacing = 4;
        statsLayout.childForceExpandWidth = true;
        statsLayout.childForceExpandHeight = false;
        statsLayout.childControlWidth = true;
        statsLayout.childControlHeight = true;
        statsLayout.childAlignment = TextAnchor.UpperLeft;

        GameObject priceGo = UIPrefabBuilderUtils.FindOrCreateChild(content.transform, "PriceText");
        UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(priceGo).preferredHeight = 24;
        TextMeshProUGUI priceText = UIPrefabBuilderUtils.GetOrAdd<TextMeshProUGUI>(priceGo);
        priceText.text = "Price: 0g";
        priceText.fontSize = 16;
        UIPrefabBuilderUtils.ApplyMedievalFont(priceText);

        GameObject sellGo = UIPrefabBuilderUtils.FindOrCreateChild(content.transform, "SellText");
        UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(sellGo).preferredHeight = 20;
        TextMeshProUGUI sellText = UIPrefabBuilderUtils.GetOrAdd<TextMeshProUGUI>(sellGo);
        sellText.text = "Sell: 70% (0g)";
        sellText.fontSize = 13;
        sellText.color = new Color(0.6f, 0.55f, 0.4f, 1f);
        UIPrefabBuilderUtils.ApplyMedievalFont(sellText);

        GameObject buyButtonGo = UIPrefabBuilderUtils.FindOrCreateChild(content.transform, "BuyButton");
        UIPrefabBuilderUtils.GetOrAdd<LayoutElement>(buyButtonGo).preferredHeight = 44;
        Image buyBg = UIPrefabBuilderUtils.GetOrAdd<Image>(buyButtonGo);
        buyBg.color = new Color(0.15f, 0.5f, 0.2f, 1f);
        Button buyButton = UIPrefabBuilderUtils.GetOrAdd<Button>(buyButtonGo);
        buyButton.targetGraphic = buyBg;

        GameObject buyLabelGo = UIPrefabBuilderUtils.FindOrCreateChild(buyButtonGo.transform, "Label");
        RectTransform buyLabelRt = UIPrefabBuilderUtils.GetOrAdd<RectTransform>(buyLabelGo);
        UIPrefabBuilderUtils.StretchFill(buyLabelRt);
        TextMeshProUGUI buyLabel = UIPrefabBuilderUtils.GetOrAdd<TextMeshProUGUI>(buyLabelGo);
        buyLabel.text = "BUY";
        buyLabel.alignment = TextAlignmentOptions.Center;
        buyLabel.fontSize = 18;
        UIPrefabBuilderUtils.ApplyMedievalFont(buyLabel);

        GameObject statLinePrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(StatLinePrefabPath);

        detailsPanelUi = UIPrefabBuilderUtils.GetOrAdd<ShopDetailsPanelUI>(rightColumn);
        UIPrefabBuilderUtils.AssignSerialized(detailsPanelUi, "emptyState", emptyState);
        UIPrefabBuilderUtils.AssignSerialized(detailsPanelUi, "content", content);
        UIPrefabBuilderUtils.AssignSerialized(detailsPanelUi, "nameHeader", nameHeader);
        UIPrefabBuilderUtils.AssignSerialized(detailsPanelUi, "itemImage", itemImage);
        UIPrefabBuilderUtils.AssignSerialized(detailsPanelUi, "flavorText", flavorText);
        UIPrefabBuilderUtils.AssignSerialized(detailsPanelUi, "statsStackContainer", statsStack.transform);
        UIPrefabBuilderUtils.AssignSerialized(detailsPanelUi, "statLinePrefab", statLinePrefabAsset);
        UIPrefabBuilderUtils.AssignSerialized(detailsPanelUi, "priceText", priceText);
        UIPrefabBuilderUtils.AssignSerialized(detailsPanelUi, "sellText", sellText);
        UIPrefabBuilderUtils.AssignSerialized(detailsPanelUi, "buyButton", buyButton);
        UIPrefabBuilderUtils.AssignSerialized(detailsPanelUi, "buyButtonLabel", buyLabel);
    }

}
