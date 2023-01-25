using UnityEngine;
using DREditor.PlayerInfo;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class CAInitialiser : MonoBehaviour
{
    [SerializeField] protected CAManager manager;
    [SerializeField] bool debug = false;
    [SerializeField] bool debugTransition = false;
    //[SerializeField] int debugResetHealth = 1;
    [Space(20)]
    [SerializeField] ArgumentBuilder toRemove;
    [SerializeField] AudioClip music;
    [SerializeField] GameObject pagePrefab;
    [SerializeField] GameObject pageAddonprefab;
    [SerializeField] protected GameObject panelPrefab;
    [SerializeField] protected GameObject stockParent;
    [SerializeField] List<int> unusedStock; //indexes of unused stock objects. Used when randomising stock placement

    [SerializeField] Camera cam;
    protected int totalPages;
    protected int totalStock;

    public static CAInitialiser instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    void Start()
    {
        if (debug)
            InitArgument(toRemove); //¬`! Eventually, this will be called by the main trial manager class, and this can be removed.
        if (debugTransition)
            PlayCA(toRemove);
    }
    public void PlayCA(ScriptableObject asset)
    {
        //toRemove = (ArgumentBuilder)asset;
        CAInitialiser.instance.CAIntro((ArgumentBuilder)asset);
    }
    public void ResetCA() => ResetArgument(toRemove);
    public void CAIntro(ArgumentBuilder asset)
    {
        toRemove = asset;
        //gameObject.SetActive(true);
        StartCoroutine(CAIntroRoutine());
    }
    IEnumerator CAIntroRoutine()
    {
        if (GlobalFade.instance != null)
            GlobalFade.instance.FadeTo(1);
        yield return new WaitForSeconds(1);
        InitArgument(toRemove);
        cam.enabled = true;
        yield break;
    }
    /// <summary>
    /// Performs visual and mechanical setup before the Closing Argument begins
    /// </summary>
    /// <param name="AB">The Closing Argument to initialise.</param>
    public virtual void InitArgument(ArgumentBuilder AB)//Setup for Closing Argument
    {
        manager.enabled = false;
        totalStock = AB.totalStock;
        unusedStock = Enumerable.Range(0, totalStock).ToList();
        ClearEmptyStock();
        foreach (ArgumentBuilder.Page pageData in AB.pages)//initialises each page
        {
            InitPage(pageData);
        }

        //Inititalisation of gameplay before the first frame
        Cursor.lockState = CursorLockMode.Locked;//Prevents janky mouse movement at beginning of minigame
        //cam = Camera.main;//¬`!! set to the trial camera once global events implemented.*
        manager.totalTime = AB.totalTime;
        manager.totalPages = totalPages;
        manager.totalQuestions = totalStock;

        manager.CheckHealth();

        SetUpStockPanel();

        //Gives the pages and the pagebar the correct rotation
        manager.pagebar.transform.rotation = Quaternion.Euler(0, 0, -3.933726f);
        manager.pages.transform.rotation = Quaternion.Euler(0, 0, -3.9f);

        SetUpPageDisplay();


        //Set timer to correct time and camera to correct position
        AB.SetTimerBasedOnDifficulty(manager.gameTimer);
        //manager.timer.text = Mathf.FloorToInt(manager.totalTime / 60).ToString() + ":" + Mathf.FloorToInt(manager.totalTime % 60).ToString();
        //cam.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 10);
        if (GlobalFade.instance != null)
            GlobalFade.instance.FadeOut(0.5f);
        if (!cam.enabled)
            cam.enabled = true;
        //Start the minigame and destroy setup object
        manager.anim.SetTrigger("Start");
        manager.enabled = true;
        SoundManager.instance.PlayMusic(music);
        //Destroy(this.gameObject);
    }
    List<GameObject> pageObjects = new List<GameObject>();
    List<GameObject> barObjects = new List<GameObject>();
    #region Initialize Pieces
    /// <summary>
    /// Creates a page of the Closing Argument.
    /// </summary>
    /// <param name="pageData">Data used to create the page.</param>
    protected void InitPage(ArgumentBuilder.Page pageData)
    {
        //Each page is instantiated 9.91 units right from the previous one
        GameObject page = Instantiate(pagePrefab, new Vector3(manager.pages.transform.position.x + totalPages * 9.91f, manager.pages.transform.position.y, manager.pages.transform.position.z), Quaternion.identity, manager.pages.transform);
        pageObjects.Add(page);
        page.GetComponent<SpriteRenderer>().sprite = pageData.pageSprite;

        foreach (ArgumentBuilder.Page.QstnPanel panelData in pageData.stock) //initialises each question panel on the page.
        {
            InitPanel(panelData, page.transform);
        }

        //Create copy of page used in the pagebar at the bottom of the screen
        GameObject miniPage = Instantiate(page, manager.pagebar.transform.position, Quaternion.Euler(0, 0, 0), manager.pagebar.transform);//clones the page and question panels created before and puts them into the pagebar
        barObjects.Add(miniPage);
        miniPage.transform.position = new Vector3(miniPage.transform.position.x + totalPages * 11.2f / 4, miniPage.transform.position.y, miniPage.transform.position.z);
        miniPage.transform.localScale = new Vector3(0.58f, 0.58f, 1);

        //Set sorting order for miniPage sprites
        miniPage.GetComponent<SpriteRenderer>().sortingOrder = 6;
        miniPage.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 6;
        for (int i = 1; i < miniPage.transform.childCount; i++)
        {
            miniPage.transform.GetChild(1).GetComponent<SpriteRenderer>().sortingOrder = 7;
        }
        for (int i = 2; i < miniPage.transform.childCount; i++)
        {
            miniPage.transform.GetChild(i).GetChild(4).GetComponent<SpriteRenderer>().sortingOrder = 8;
        }

        //add extras to the page that we don't want on the miniPage
        GameObject addons = Instantiate(pageAddonprefab, page.transform);
        addons.transform.SetAsFirstSibling();

        totalPages++;


    }

    /// <summary>
    /// Creates a mystery panel on the relevant page. Called by <c>InitPage()</c>
    /// </summary>
    /// <param name="panelData">Data used to create the panel.</param>
    /// <param name="page">The transform of the panel's parent page.</param>
    protected virtual void InitPanel(ArgumentBuilder.Page.QstnPanel panelData, Transform page)
    {
        //Create panel visually
        GameObject qstnPanel = Instantiate(panelPrefab, page.position, Quaternion.identity, page);
        qstnPanel.transform.position += new Vector3(panelData.panelPos.x, panelData.panelPos.y, 0) * 3 / 5; //Set position of panel. page scale = 0.8, pages parent object scale = 0.75, 0.75*0.8= 3/5
        qstnPanel.GetComponent<SpriteRenderer>().sprite = panelData.panelShape; //Gives the polygon collider the correct shape when it's attached
        qstnPanel.AddComponent<PolygonCollider2D>();
        qstnPanel.GetComponent<PolygonCollider2D>().isTrigger = true;
        qstnPanel.GetComponent<SpriteMask>().sprite = panelData.panelShape;
        Destroy(qstnPanel.GetComponent<SpriteRenderer>() );

        //Set position of question mark
        Transform symbol = qstnPanel.transform.GetChild(4);
        symbol.localPosition = panelData.symbolOffset;
        symbol.localScale = new Vector2(symbol.localScale.x * panelData.symbolScale.x, symbol.localScale.y * panelData.symbolScale.y);

        //Set data in panel component
        QuestionPanel panelComp = qstnPanel.GetComponent<QuestionPanel>();

        panelComp.questionText = panelData.questionText;
        panelComp.answer = InitStock(panelData);
    }
    

    /// <summary>
    /// Creates an panel in the stock. Called by <c>InitPanel()</c> after a question panel's created to create the corresponding stock.
    /// </summary>
    /// <param name="panelData">The data used to create the stock panel.</param>
    /// <returns>Created stock.</returns>
    protected virtual CAStock InitStock(ArgumentBuilder.Page.QstnPanel panelData)
    {
        int randomIndex = Random.Range(0, unusedStock.Count); //index of the stock index to use
        int stockIndex = unusedStock[randomIndex];
        unusedStock.RemoveAt(randomIndex);

        //Create stock
        CAStock eve = stockParent.transform.GetChild(stockIndex).GetComponent<CAStock>();
        eve.sprite = panelData.panelShape; //Sprite to be displayed by activePanel when the stock is hovered

        //Setup stock sprite
        Transform spriteObj = eve.transform.GetChild(3);
        spriteObj.GetComponent<SpriteRenderer>().sprite = eve.sprite;
        spriteObj.localPosition = panelData.stockPosition;
        spriteObj.localScale = panelData.stockScale;

        //Set data
        eve.flavourText = panelData.flavourText;
        eve.remainingLocks = panelData.noOfLocks;
        eve.activePanelScale = panelData.activeScale;
        eve.activePanelPos = panelData.activePosition;

        //Is the stock locked or not?
        if (eve.remainingLocks == 0)
        {
            eve.remainingLocks = -1;//signifies that this stock has been unlocked
            eve.GetComponent<Animator>().SetTrigger("Unlocked");
        }
        else
        {
            eve.selectable = false;
        }

        return eve;
    }

    protected void UpdateStockNavi()
    {

    }
    #endregion

    /// <summary>
    /// Removes any unused stock objects from the scene, and alters stock navigation to reflect this.
    /// </summary>
    protected virtual void ClearEmptyStock()
    {
        CAStock[] stocks = stockParent.transform.GetComponentsInChildren<CAStock>();
        if (stocks.Length == totalStock)
            return;
        for (int i = totalStock; i < 10; i++)
        {
            CAStock eve = stockParent.transform.GetChild(i).GetComponent<CAStock>();

            for (int j = 0; j < 4; j++)
            {
                try
                {
                    CAStock otherEve = eve.nodeDirections[j];
                    if (j <= 1)
                        otherEve.nodeDirections[j + 2] = eve.nodeDirections[j + 2];
                    else
                        otherEve.nodeDirections[j - 2] = eve.nodeDirections[j - 2];
                }

                catch //If nodeDirections[j+/-2] is a null pointer
                {

                }
            }
            Destroy(eve.gameObject);
        }
    }
    public virtual void ResetArgument(ArgumentBuilder AB)//RESET FOR CA
    {
        manager.enabled = false;
        manager.UnSelectStockO();
        manager.activePanelParent.GetComponent<Animator>().Rebind();
        manager.StartingReset();
        manager.ResetStocks();
        manager.StopMyPain();
        manager.pages.transform.localPosition = new Vector3(0, 0.93f, 0);
        manager.pagebar.transform.localPosition = new Vector3(-0.49f, -3.3f, 0);
        manager.ResetPageDiffCounter();
        foreach (GameObject g in pageObjects)
            Destroy(g);
        pageObjects.Clear();
        foreach (GameObject g in barObjects)
            Destroy(g);
        barObjects.Clear();

        totalPages = 0;
        totalStock = 0;
        
        PlayerInfo.instance.CurrentHealth = PlayerInfo.instance.MaxHealth;

        manager.pagebar.transform.rotation = Quaternion.Euler(0, 0, 0);
        manager.pages.transform.rotation = Quaternion.Euler(0, 0, 0);

        InitArgument(toRemove);
        

        //Start the minigame and destroy setup object
        //manager.anim.SetTrigger("Start");
        manager.enabled = true;
        manager.allowInput = true;
        manager.reticle.allowInput = true;
        StartCoroutine(PainEnd());
        //Debug.LogWarning(manager.pages.transform.GetChild(0).localScale);
        //Destroy(this.gameObject);
    }
    IEnumerator PainEnd()
    {
        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        Transform pageStart = manager.pages.transform.GetChild(0);
        pageStart.localScale = new Vector3(0.65f, 0.65f, 1);
        //Debug.LogWarning(pageStart.localScale);
        Color colour = pageStart.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().color; //Make the page's mask transparent
        colour.a = 0;
        pageStart.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().color = colour;

        
        //SpriteRenderer eveBorder = activePanelParent.transform.GetChild(0).GetComponent<SpriteRenderer>();
        //eveBorder.sprite = currEveSprs[0];
        yield break;
    }
    void SetUpStockPanel()
    {
        //Setup for initial stock panel
        manager.currentStock.gameObject.GetComponent<Animator>().SetBool("Hovered", true);
        if (manager.currentStock.remainingLocks <= 0)
        {
            manager.currentStock.gameObject.GetComponent<Animator>().SetInteger("State", 1);

            manager.activePanel.sprite = manager.currentStock.sprite;
            manager.activePanel.transform.localScale = manager.currentStock.activePanelScale;
            manager.activePanel.transform.localPosition = manager.currentStock.activePanelPos;
            manager.activePanelText.text = manager.currentStock.flavourText;

            manager.activePanelParent.GetComponent<Animator>().SetInteger("State", 1);

        }
        else
        {
            if (manager.currentStock.remainingLocks == 1)
                manager.activePanelText.text = "Solve another panel to unlock.";
            else
                manager.activePanelText.text = "Solve another " + manager.currentStock.remainingLocks.ToString() + " panels to unlock.";
        }
    }
    void SetUpPageDisplay()
    {
        //The first page to be displayed is displayed prominently
        Transform pageStart = manager.pages.transform.GetChild(0);
        //Debug.LogWarning(pageStart.gameObject.name);
        pageStart.position = new Vector3(pageStart.transform.position.x, pageStart.transform.position.y, -1);
        pageStart.localScale = new Vector3(0.65f, 0.65f, 1);
        //Debug.LogWarning(pageStart.localScale);
        Color colour = pageStart.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().color; //Make the page's mask transparent
        colour.a = 0;
        pageStart.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().color = colour;

        //Set up display for which page you're on
        manager.pageCounter.text = "<color=#E3D13E>" + 1.ToString() + "</color><size=70%> /" + totalPages.ToString();
        manager.pageCounter.transform.GetChild(0).GetComponent<TextMeshPro>().text = 1.ToString() + "<size=70%> /" + totalPages.ToString();

        //Set the end position for the pagebar and the pages for when movement occurs
        manager.barEndPos = manager.pagebar.transform.position;
        manager.pageEndPos = manager.pages.transform.position;
        if (toRemove.pages.Count == 1)
        {
            manager.arrows.gameObject.SetActive(false);//disables arrows if there's only one page
        }
        else
        {
            manager.arrows.GetChild(0).gameObject.SetActive(false); //disables the left arrow
        }
    }
}
