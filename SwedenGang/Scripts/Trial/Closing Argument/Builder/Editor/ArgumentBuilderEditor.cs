using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArgumentBuilder))]
public class ArgumentBuilderEditor : Editor
{
    private ArgumentBuilder AB;
    private PreviewRenderUtility _previewScene;
    private Texture _previewTexture;
    private int totalStock = 0; //tracks total stock added.
    private bool updatePreview = true; //Whether or not the preview needs to be updated
    private int currentPage = 1; //Page currently being previewed
    private int currentStock = 1; //Stock currently being previewed


    
    private void OnEnable() => AB = target as ArgumentBuilder;

    #region Preview
    public override bool HasPreviewGUI()
    {
        InitPreview();
        return true;
    }

    private void InitPreview() 
    {
        if (updatePreview) 
        {
            if (_previewScene != null)
            {
                _previewScene.Cleanup();
            }
            totalStock = AB.totalStock = GetTotalStock(); //Gets the total stock for use by this script and during minigame setup
            _previewScene = new PreviewRenderUtility();
            Camera cam = _previewScene.camera;
            cam.transform.position = new Vector3(0, 2, -20);
            cam.transform.LookAt(Vector3.zero);
            cam.fieldOfView = 30f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000;

            var obj = Resources.Load<GameObject>("Trial/Closing Argument/PCA");
            if(obj == null)
            {
                return;
            }
            _previewTexture = CreatePreviewTexture(obj);

            updatePreview = false;
        }
    }

    private Texture CreatePreviewTexture(GameObject obj) 
    {
        _previewScene.BeginPreview(new Rect(0, 0, 1920, 1080), GUIStyle.none);

        var joe = _previewScene.InstantiatePrefabInScene(obj);
        joe.transform.position = new Vector3(0, 0, 0);
        if( AB.pages.Count > 0 )
            joe.transform.GetChild(0).GetComponent<CAPreviewInit>().InitArgument(AB,currentPage,currentStock);
        _previewScene.camera.Render();
        return _previewScene.EndPreview();
    }

    private int GetTotalStock() 
    {
        int stockCount = 0;
        for (int i = 0; i < AB.pages.Count; i++)
        {
            for (int j = 0; j < AB.pages[i].stock.Count; j++)
            {
                stockCount++;
            }
        }
        return stockCount;
    }

    private int GetStockIndex(ArgumentBuilder.Page.QstnPanel stock)
    {
        int stockCount = 0;
        for (int i = 0; i < AB.pages.Count; i++)
        {
            for (int j = 0; j < AB.pages[i].stock.Count; j++)
            {
                stockCount++;
                if(AB.pages[i].stock[j].Equals(stock))
                {
                    return stockCount;
                }
            }
        }
        return -1;
    }

    private int GetStockIndex(ArgumentBuilder.Page page,bool first)
    {
        int stockCount = 0;
        for (int i = 0; i < AB.pages.Count; i++)
        {
            if (AB.pages[i].Equals(page))
            {
                if (first)
                {
                    if (AB.pages[i].stock.Count != 0)
                        stockCount += 1;
                    return stockCount;
                }
                else
                {
                    stockCount += AB.pages[i].stock.Count;
                    return stockCount;
                }
            }
            stockCount += AB.pages[i].stock.Count;
        }
        return -1; //This should never happen but all code paths must return a value.
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (Event.current.type == EventType.Repaint && _previewTexture != null)
        {
            GUI.DrawTexture(r, _previewTexture,ScaleMode.ScaleToFit,false);
        }
    }

    public override void OnPreviewSettings() 
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (AB.pages.Count == 0)
            {
                GUILayout.Label("No Page to Preview");
            }
            else
            {
                if (currentPage > 1)
                {
                    GUILayout.Label("Previous page:");
                    if (GUILayout.Button("<"))
                    {
                        currentPage -= 1;
                        updatePreview = true;
                    }
                }
                if (currentPage < AB.pages.Count)
                {
                    GUILayout.Label("Next page:");
                    if (GUILayout.Button(">"))
                    {
                        currentPage += 1;
                        updatePreview = true;
                    }
                }
                GUILayout.Label("Previewing page " + currentPage);
            }

            if (totalStock == 0) 
            {
                GUILayout.Label("No Stock to Preview");
            }
            else
            {
                if (currentStock > 1)
                {
                    GUILayout.Label("Previous stock:");
                    if (GUILayout.Button("<"))
                    {
                        currentStock -= 1;
                        updatePreview = true;
                    }
                }
                if (currentStock < totalStock)
                {
                    GUILayout.Label("Next stock:");
                    if (GUILayout.Button(">"))
                    {
                        currentStock += 1;
                        updatePreview = true;
                    }
                }
                GUILayout.Label("Previewing stock " + currentStock);
            }

        }
        
    }

    #endregion

    private void OnDisable()
    {
        if (_previewScene != null)
        {
            _previewScene.Cleanup();
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Closing Argument Builder", EditorStyles.boldLabel);
        ArgumentForm();
        EditorUtility.SetDirty(AB);
    }

    public void ArgumentForm()
    {
        AB.times = BuilderEditor.DisplayTimerSettings(AB.times);
        using (new EditorGUILayout.VerticalScope())
        {
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.Label(new GUIContent("Total Time (seconds)", "How long the player has to beat the minigame."), GUILayout.Width(150));
                AB.totalTime = EditorGUILayout.FloatField(AB.totalTime, GUILayout.Width(100));
                if (EditorGUI.EndChangeCheck())
                {
                    updatePreview = true;
                }
            }

            if (AB.pages.Count != 0)
            {
                for (int i = 0; i < AB.pages.Count; i++)
                {
                    ArgumentBuilder.Page page = AB.pages[i];
                    using (new EditorGUILayout.HorizontalScope("Box"))
                    {
                        ShowPageInfo(page, i + 1);
                    }
                }
            }

            if (GUILayout.Button(new GUIContent("Add Page", "Add a new page."), GUILayout.Width(100)))
            {
                AB.pages.Add(new ArgumentBuilder.Page());
                updatePreview = true;
            }
            
        }
    }


    public void ShowPageInfo(ArgumentBuilder.Page page, int pageNumber)
    {
        using (new EditorGUILayout.VerticalScope())
        {
            EditorGUILayout.LabelField("Page " + pageNumber.ToString(), EditorStyles.boldLabel);
            GUILayout.Space(10);

            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(new GUIContent("Page Sprite:", "The page's sprite."), GUILayout.Width(120));
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUI.BeginChangeCheck();

                        page.pageSprite = (Sprite)EditorGUILayout.ObjectField(page.pageSprite, typeof(Sprite), false, GUILayout.Width(120));

                        if (EditorGUI.EndChangeCheck())
                        {
                            updatePreview = true;
                        }
                    }
                }
                GUILayout.Label(AssetPreview.GetAssetPreview(page.pageSprite));
            }

            if (page.stock.Count == 0)
            {
                if (GUILayout.Button(new GUIContent("Add Stock", "Add a new panel to this page and the stock."), GUILayout.Width(100)))
                {
                    page.stock.Add(new ArgumentBuilder.Page.QstnPanel());
                    updatePreview = true;
                }
            }

            if (page.stock.Count != 0)
            {
                for (int j = 0; j < page.stock.Count; j++)
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        ArgumentBuilder.Page.QstnPanel panel = page.stock[j];
                        using (new EditorGUILayout.HorizontalScope("Box"))
                        {
                            using (new EditorGUILayout.VerticalScope())
                            {

                                ShowStockInfo(panel);

                                if (GUILayout.Button(new GUIContent("Remove Stock", "Remove the current stock panel."), GUILayout.Width(100)))
                                {
                                    int stockIndex = GetStockIndex(panel); //get index of removed stock
                                    page.stock.Remove(panel);
                                    updatePreview = true;
                                    
                                    if(currentStock !=1 && stockIndex < currentStock)
                                        currentStock--;
                                }
                            }
                        }
                        GUILayout.Space(10);
                    }

                }
            }

            if (page.stock.Count != 0 && totalStock < 10)
            {
                if (GUILayout.Button(new GUIContent("Add Stock", "Add a new panel to this page and the stock.."), GUILayout.Width(100)))
                {
                    page.stock.Add(new ArgumentBuilder.Page.QstnPanel());
                    updatePreview = true;
                }
            }

            if (GUILayout.Button("Remove Page", GUILayout.Width(100)))
            {
                if (currentPage != 1 && (pageNumber < currentPage || currentPage == AB.pages.Count))
                    currentPage -= 1;

                if(currentStock != 1)
                {
                    int highPage;
                    if (currentStock > (highPage = GetStockIndex(page, false)))
                        currentStock -= page.stock.Count;
                    else
                    {
                        if (currentStock >= GetStockIndex(page, true))
                            currentStock = highPage - page.stock.Count;
                    }
                }

                AB.pages.Remove(page);
                if(currentStock <= 0)
                {
                    currentStock = 1;
                }
                updatePreview = true;

            }
        }
    }

    public void ShowStockInfo(ArgumentBuilder.Page.QstnPanel panel)
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.Space(20);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(new GUIContent("Sprite: ", "The sprite for the comic panel's shape, the stock panel's image, and the active panel image."), GUILayout.Width(130));
            panel.panelShape = (Sprite)EditorGUILayout.ObjectField(panel.panelShape, typeof(Sprite), false, GUILayout.Width(120));

        }
        GUILayout.Label(AssetPreview.GetAssetPreview(panel.panelShape));

        EditorGUILayout.LabelField("Comic Panel", EditorStyles.helpBox,GUILayout.Width(70));

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(new GUIContent("Symbol Scale: ", "Size of the question mark/checkmark on the question panel."), GUILayout.Width(90));
            panel.symbolScale = EditorGUILayout.Vector2Field("", panel.symbolScale, GUILayout.Width(100));
            GUILayout.Space(15);
            GUILayout.Label(new GUIContent("Position:", "The position of the panel relative to the centre of the comic page, measured in unity units"), GUILayout.Width(55));
            panel.panelPos = EditorGUILayout.Vector2Field("", panel.panelPos, GUILayout.Width(100));
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(new GUIContent("Symbol Offset: ", "Question mark/checkmark's position relative to the centre of the question panel."), GUILayout.Width(90));
            panel.symbolOffset = EditorGUILayout.Vector2Field("", panel.symbolOffset, GUILayout.Width(100));
            GUILayout.Space(15);
            EditorStyles.textField.wordWrap = true;
            GUILayout.Label(new GUIContent("Hint:", "Text displayed when the question panel is hovered."), GUILayout.Width(30));
            
            panel.questionText = EditorGUILayout.TextField(panel.questionText,GUILayout.Width(200),GUILayout.Height(35));
        }

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Stock Panel", EditorStyles.helpBox,GUILayout.Width(68));

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(new GUIContent("Number of Locks:", "Number of panels that need to be unlocked before this can be selected."), GUILayout.Width(110));
            panel.noOfLocks = EditorGUILayout.IntSlider(panel.noOfLocks, 0, 9, GUILayout.Width(150));
        }

        GUILayout.Space(5);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(new GUIContent("Stock Hint:", "Text displayed in the text box when the stock panel is hovered."), GUILayout.Width(70));
            panel.flavourText = EditorGUILayout.TextField(panel.flavourText, GUILayout.Width(200));
        }

        GUILayout.Space(1);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(new GUIContent("Stock Sprite Scale: ", "Size of the stock sprite"), GUILayout.Width(115));
            panel.stockScale = EditorGUILayout.Vector2Field("", panel.stockScale, GUILayout.Width(100));
            GUILayout.Label(new GUIContent("Active Sprite Scale: ", "Size of the active panel sprite."), GUILayout.Width(115));
            panel.activeScale = EditorGUILayout.Vector2Field("", panel.activeScale, GUILayout.Width(100));
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(new GUIContent("Stock Sprite Offset: ", "Stock sprite's position relative to the centre of the stock panel."), GUILayout.Width(115));
            panel.stockPosition = EditorGUILayout.Vector2Field("", panel.stockPosition, GUILayout.Width(100));
            GUILayout.Label(new GUIContent("Active Sprite Offset: ", "Active panel sprite's position relative to the centre of the active panel."), GUILayout.Width(115));
            panel.activePosition = EditorGUILayout.Vector2Field("", panel.activePosition, GUILayout.Width(100));
        }

        if (EditorGUI.EndChangeCheck())
        {
            updatePreview = true;
        }
    }
}
