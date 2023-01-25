using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DREditor.PlayerInfo;
using UnityEngine.InputSystem;
using System.Linq;
using DREditor.EventObjects;

public class CAManager : MinigameManagerBase
{
    [SerializeField] BoolWithEvent InMenu = null;
    [Header("Stock")]
    public CAStock currentStock; //the currently hovered stock
    CAStock firstStock;
    public SpriteRenderer activePanel; //the sprite renderer used to display the currently hovered stock. Child of Stock Display because the sprite needs to be scaled separately
    public GameObject activePanelParent;
    public TMP_Text activePanelText; //text next to the activePanel
    public bool isSelected;
    [SerializeField] List<Sprite> currEveSprs = new List<Sprite>(); //list of borders for the activePanel
    [SerializeField] List<Sprite> eveBorders = new List<Sprite>(); //list of borders for the stock panels


    [Header("General")]
    [Space(20)]
    public TMP_Text timer;
    public float totalTime; //total time left in the minigame
    public int totalQuestions = 0;
    public TrialTimer gameTimer;
    [SerializeField] int totalAnswered = 0;
    [SerializeField] List<KeyCode> controls = new List<KeyCode>{KeyCode.Return, KeyCode.Escape, KeyCode.Mouse0, KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.Q, KeyCode.E};
    public bool allowInput = true;
    public Animator anim; //animation controller for the correct/incorrect screen and start/end screens
    [SerializeField] Transform hearts; //parent object of the health bar
    private bool waitForEnd = false;
    [HideInInspector] public bool endCheck = false;


    [Header("Pages")]
    [Space(20)]

    public GameObject pagebar;
    public GameObject pages; //parent object of the pages
    [SerializeField] int currentIndex = 0; //current page index
    public CAReticle reticle;
    public int totalPages;
    public TMP_Text pageCounter; //text display for the current page number
    public Transform arrows; //arrows next to the pages
    [SerializeField] Transform pagebarArrows; //arrows on the pagebar
    [SerializeField] Sprite checkMark;
    [SerializeField] Sprite questionMark;

    private int pageDiffCounter; //the difference between the currentIndex and the actual page index
    [HideInInspector] public Vector3 barEndPos = new Vector3(0,0,0); //end position of the pagebar when a new page is selected 
    [HideInInspector] public Vector3 pageEndPos = new Vector3(0, 0, 0); //endposition of the pages when a new page is selected 
    private Coroutine co; //used with the pagemovement coroutine
    private Coroutine timeCo; //used with coroutines that wait a certain amount of time before performing the action

    [Header("I Got It Section")]
    [SerializeField] Canvas ansCanvas = null;
    [SerializeField] Animator ansAnimator = null;
    [SerializeField] string ansAnimName = "Conclusion";
    [SerializeField]AudioClip ansSound;
    [SerializeField]AudioClip ansVO;
    [Tooltip("How long the game waits until it plays the \"Here's what happened\" animation")]
    [SerializeField] float menuFadeWaitTime = 0.5f;

    [Header("Sound")]
    [Space(20)]
    [SerializeField] AudioClip start;
    [SerializeField] AudioClip stockScroll = null;
    [SerializeField] AudioClip stockSelect = null;
    [SerializeField] AudioClip stockFly = null;
    [SerializeField] AudioClip stockDeselect = null;
    [SerializeField] AudioClip stockLocked = null;
    [SerializeField] AudioClip stockUnlocked = null;
    [SerializeField] AudioClip pageTurn = null;

    [SerializeField] AudioClip correct = null;
    [SerializeField] AudioClip incorrect = null;
    [SerializeField] AudioClip end;

    private TextMeshPro timerShadow;
#if ENABLE_INPUT_SYSTEM
    DRControls _controls;
#endif
    private void Awake()
    {
        firstStock = currentStock;
        timerShadow = timer.transform.GetChild(0).GetComponent<TextMeshPro>();
#if ENABLE_INPUT_SYSTEM
        _controls = new DRControls();
#endif
    }
    public void ResetPageDiffCounter()
    {
        pageDiffCounter = 0;
        currentIndex = 0;
    }
    private void Start()
    {
        if (ansCanvas != null && ansCanvas.enabled)
            ansCanvas.enabled = false;
        StartCoroutine(LateStart());
    }
    IEnumerator LateStart()
    {
        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        EG_GameOver.DisableTDMContinue();
        yield break;
    }
    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        _controls.Enable();
#endif
        EnableControls();
        TrialTimer.TimeUp += TimeUpFunc;
        EG_GameOver.OnContinue += ContinueRestart;
        //EG_GameOver.DisableTDMContinue();
    }
    private void OnDisable()
    {
        DisableControls();
#if ENABLE_INPUT_SYSTEM
        _controls.Disable();
#endif
        TrialTimer.TimeUp -= TimeUpFunc;
        EG_GameOver.OnContinue -= ContinueRestart;
    }
    void EnableControls()
    {
        _controls.Minigame.Move.performed += StockMovement;
        _controls.Minigame.Select.performed += SelectStock;
        _controls.Minigame.ShuffleLeft.performed += CallMovePages;
        _controls.Minigame.ShuffleRight.performed += CallMovePages;
        _controls.Minigame.Choose.performed += IsStockCorrect;
        _controls.Minigame.BulletList.performed += UnSelectStock;
        _controls.UI.Cancel.performed += UnSelectStock;
        _controls.UI.Controls.started += CallControls;
    }
    void DisableControls()
    {
        _controls.Minigame.Move.performed -= StockMovement;
        _controls.Minigame.Select.performed -= SelectStock;
        _controls.Minigame.ShuffleLeft.performed -= CallMovePages;
        _controls.Minigame.ShuffleRight.performed -= CallMovePages;
        _controls.Minigame.Choose.performed -= IsStockCorrect;
        _controls.Minigame.BulletList.performed -= UnSelectStock;
        _controls.UI.Cancel.performed -= UnSelectStock;
        _controls.UI.Controls.started -= CallControls;
    }
    void CallControls(InputAction.CallbackContext context)
    {
        if (!allowInput)
            return;
        DisableControls();
        GameManager.CallControls(allowInput);
        allowInput = false;
        StartCoroutine(WaitForControlsEnd());
    }
    IEnumerator WaitForControlsEnd()
    {
        while (InMenu.Value)
        {
            yield return null;
        }
        EnableControls();
        allowInput = true;
        yield break;
    }
    void Update()
    {
        if (totalQuestions > totalAnswered)
        {
            totalTime -= Time.deltaTime;

            timerShadow.text = timer.text; //update drop shadow
        }
        else if(!waitForEnd) //once every question's been answered
        {
            //allowInput = false;
            //waitForEnd = true;
            //anim.SetTrigger("End"); //¬`! I don't like this on prinicple but not a huge deal if it can't be fixed.
            
            // On Minigame Finished/Passed Code? might be at the below if 
        }
        /*
        else if(endCheck) //once end animation's played.
        {
            endCheck = false;
            EndMinigame();
        }
        */
    }
    void TimeUpFunc()
    {
        StartCoroutine(TimeUpRoutine());
    }
    IEnumerator TimeUpRoutine()
    {
        while (!allowInput)
        {
            yield return null;
        }

        allowInput = false;
        reticle.allowInput = false;
        gameTimer.StopTimer();
        TrialDialogueManager.CallPlayerHasDied();
        yield break;
    }
    void ContinueRestart()
    {
        GetComponentInChildren<CAInitialiser>().ResetCA();
    }
    /// <summary>
    /// Updates hearts to display the correct amount of health.
    /// </summary>
    public void CheckHealth()
    {
        int health = PlayerInfo.instance.CurrentHealth;
        int fullHearts = health / 2;

        for (int i = 0; i < fullHearts; i++) //¬`! This for should only exist if we have healing
        {
            hearts.GetChild(i).GetComponent<SpriteRenderer>().sprite = hearts.GetChild(i).GetComponent<Hearts>().sprites[2];
        }
        for (int i = fullHearts; i < 5; i++)
        {
            hearts.GetChild(i).GetComponent<SpriteRenderer>().sprite = hearts.GetChild(i).GetComponent<Hearts>().sprites[0];
        }

        if(health % 2 == 1)
            hearts.GetChild(fullHearts).GetComponent<SpriteRenderer>().sprite = hearts.GetChild(fullHearts).GetComponent<Hearts>().sprites[1];
        else if(health == 0)
        {
            //¬`!!!lose
            TimeUpFunc();
        }
    }

    /// <summary>
    /// Calls <code>MovePages()</code> after deciding which direction to move from the input. 
    /// </summary>
    /// <param name="context">Player Input information.</param>
    public void CallMovePages(InputAction.CallbackContext context) //The input should just call the function but it doesn't work with how I made it so it has to go through this instead. Easy fix for next time though.
    {
        
        if (!context.performed || !allowInput) return;

        int dir; //direction for the pages to move. -1 = left, 1 = right.
        if (context.action.name.Substring(context.action.name.Length - 4) == "Left")
        {
            if (currentIndex + pageDiffCounter == 0) 
                return;
            dir = -1;
            pagebarArrows.GetChild(0).GetComponent<Animator>().SetTrigger("Click");
        }
        else
        {
            if (currentIndex + pageDiffCounter + 1 == totalPages) 
                return;
            dir = 1;
            pagebarArrows.GetChild(1).GetComponent<Animator>().SetTrigger("Click");
        }

        SoundManager.instance.PlaySFX(pageTurn);
        if (co != null)
            StopCoroutine(co);
        co = StartCoroutine(MovePages(dir));
        //pagebarArrows.GetChild(0).GetComponent<Animator>().SetTrigger("Click");
            
    }
    public void StopMyPain()
    {
        if (co != null)
            StopCoroutine(co);
    }

    public void SelectStock(InputAction.CallbackContext context)
    {
        if (!context.performed || !allowInput)
            return;

        if (currentStock.remainingLocks > 0)
        {
            PlaySFX(stockLocked);
        }
        else if (isSelected)
        {
            isSelected = false;//undoes the selection
            activePanelParent.GetComponent<Animator>().SetBool("Clicked", false);
            currentStock.GetComponent<Animator>().SetBool("Clicked", false);
            PlaySFX(stockDeselect);
        }
        else if (currentStock.selectable && !isSelected)
        {
            isSelected = true;
            activePanelParent.GetComponent<Animator>().SetBool("Clicked", true);
            currentStock.GetComponent<Animator>().SetBool("Clicked", true);

            PlaySFX(stockSelect);
        }
    }
    /// <summary>
    ///  To be Escape or cancel
    /// </summary>
    /// <param name="context"></param>
    public void UnSelectStock(InputAction.CallbackContext context)
    {
        if (!context.performed || !allowInput)
            return;

        if (isSelected)
        {
            isSelected = false;//undoes the selection
            activePanelParent.GetComponent<Animator>().SetBool("Clicked", false);
            currentStock.GetComponent<Animator>().SetBool("Clicked", false);
            PlaySFX(stockDeselect);
        }
    }
    public void UnSelectStockO()
    {
        if (isSelected)
        {
            isSelected = false;//undoes the selection
            activePanelParent.GetComponent<Animator>().SetBool("Clicked", false);
            currentStock.GetComponent<Animator>().SetBool("Clicked", false);
        }
    }

    /// <summary>
    /// Checks if the currently selected stock is the answer for the panel being shot at and plays the relevant animation.
    /// </summary>
    /// <param name="context"></param>
    public void IsStockCorrect(InputAction.CallbackContext context)
    {
        if (reticle.selectedPanel == null || !context.performed || !allowInput)
            return;

        QuestionPanel selection = reticle.selectedPanel;
        if (isSelected)
        {
            PlaySFX(stockFly);
            if (selection.answer == currentStock)
                StartCoroutine(PanelShootAnim("CorrectAnim"));
            else
                StartCoroutine(PanelShootAnim("IncorrectAnim"));
        }
    }

    /// <summary>
    /// Moves focus to another stock panel. 
    /// </summary>
    /// <param name="context">Player input information.</param>
    public void StockMovement(InputAction.CallbackContext context)
    {
        if (!context.performed || isSelected || !allowInput)
        {
            return;
        }

        Vector2 input = context.ReadValue<Vector2>();
        int dirIndex;

        if (input.x == 0)
        {
            if (input.y > 0)
                dirIndex = 0;
            else
                dirIndex = 2;
        }
        else
        {
            if (input.x > 0)
                dirIndex = 1;
            else
                dirIndex = 3;
        }

        if (currentStock.nodeDirections[dirIndex] == null) //if direction pressed leads to null pointer.
            return;
        //"move" selection to new stock
        currentStock.gameObject.GetComponent<Animator>().SetBool("Hovered", false);
        currentStock = currentStock.nodeDirections[dirIndex];
        currentStock.gameObject.GetComponent<Animator>().SetBool("Hovered", true);

        //activePanel visual changes
        activePanelParent.GetComponent<Animator>().SetTrigger("Change"); //¬`!!
        SpriteRenderer eveBorder = activePanelParent.transform.GetChild(0).GetComponent<SpriteRenderer>();
        eveBorder.sprite = currEveSprs[0];
        PlaySFX(stockScroll);

        //Change activePanel visuals based on status of hovered stock
        if (currentStock.selectable)//stock is unlocked and unsolved
        {
            activePanelText.text = currentStock.flavourText;
            activePanel.sprite = currentStock.sprite;
            activePanel.transform.localPosition = currentStock.activePanelPos;
            activePanel.transform.localScale = currentStock.activePanelScale;
            activePanelParent.GetComponent<Animator>().SetInteger("State", 1);
        }

        else if(currentStock.remainingLocks == -2) //stock is solved
        {
            activePanelText.text = currentStock.flavourText;
            activePanel.sprite = currentStock.sprite;
            activePanel.transform.localPosition = currentStock.activePanelPos;
            activePanel.transform.localScale = currentStock.activePanelScale;
            activePanelParent.GetComponent<Animator>().SetInteger("State", 2);
        }

        else if (currentStock.remainingLocks == 0) //stock has 0 locks remaining and hasn't been hovered yet.
        {
            StartCoroutine(StockUnlock());

            /**eventDisplay.GetComponent<Animator>().enabled = true;    ¬`!! This is all just a huge mess lol. look over this again once we have unlock animation
            //¬`!set the event display border to the default
            eventDisplay.GetComponent<Animator>().SetTrigger("Unlock");
            if (timeCo != null)
                StopCoroutine(timeCo);
            timeCo = StartCoroutine(EventUnlock());**/
        }
    
        else 
        {
            if (currentStock.remainingLocks == 1)//stock has 1 lock left
                activePanelText.text = "Solve another panel to unlock.";
            else //stock has multiples locks left
                activePanelText.text = "Solve another " + currentStock.remainingLocks.ToString() + " panels to unlock.";

            activePanel.GetComponentInParent<Animator>().SetInteger("State", 0);
            //activePanel.sprite = null;
        }
    }

    /// <summary>
    /// Moves the focus to the next/previous page.
    /// </summary>
    /// <param name="pageDifference">Whether the next page is the next one or the previous one (1 & -1 respectively)</param>
    /// <returns></returns>
    protected IEnumerator MovePages(int pageDifference) //moves the pages and pagebar when the current page is changed
    {
        reticle.GetComponent<CAReticle>().DisableMech(); //Prevents collision with question panels during page movement.
        arrows.gameObject.SetActive(false);
        pageDiffCounter += pageDifference; //makes sure the current page (the one that will be shown to the player) is always correct, no matter the order keys are pressed

        barEndPos = new Vector3(-2.80f * pageDifference + barEndPos.x, 0.2f * pageDifference + barEndPos.y, 0);
        pageEndPos = new Vector3(-9.91f * pageDifference + pageEndPos.x, 0.63f * pageDifference + pageEndPos.y, 0);
        Transform currentPage = pages.transform.GetChild(currentIndex + pageDiffCounter - pageDifference);
        Transform toBepage = pages.transform.GetChild(currentIndex + pageDiffCounter);
        SpriteRenderer currentPageShadow = currentPage.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
        SpriteRenderer toBepageShadow = toBepage.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();

        //Hides all textboxes on current page
        for (int i = 3; i < currentPage.transform.childCount; i++)
        {
            currentPage.GetChild(i).gameObject.GetComponent<Animator>().SetInteger("Shown", -1);
        }

        //move the pages and pagebar
        while (pagebar.transform.position.x != barEndPos.x || pages.transform.position.x != pageEndPos.x || currentPage.localScale.x != 0.4f)
        {
            pagebar.transform.position = Vector3.MoveTowards(pagebar.transform.position, barEndPos, 0.1f*Time.deltaTime*200);
            pages.transform.position = Vector3.MoveTowards(pages.transform.position, pageEndPos, 0.4f * Time.deltaTime*200);
            currentPage.localScale = Vector3.MoveTowards(currentPage.localScale, new Vector3(0.4f, 0.4f, 1), 0.1f * Time.deltaTime*200);
            toBepage.localScale = Vector3.MoveTowards(toBepage.localScale, new Vector3(0.65f, 0.65f, 1), 0.1f * Time.deltaTime*200);
            //change the transparency of the page masks
            currentPageShadow.color = Vector4.MoveTowards(currentPageShadow.color, new Vector4(0, 0, 0, 1), 0.1f * Time.deltaTime*200);
            toBepageShadow.color = Vector4.MoveTowards(toBepageShadow.color, new Vector4(0, 0, 0, 0), 0.1f * Time.deltaTime*200);
            yield return null;
        }


        //setup after the movement's finished
        currentPageShadow.color = new Vector4(0, 0, 0, 1);
        toBepageShadow.color = new Vector4(0, 0, 0, 0);

        barEndPos = pagebar.transform.position;
        currentIndex += pageDiffCounter;
        pageDiffCounter = 0;

        pageCounter.text = "<color=#E3D13E>" + (currentIndex + 1).ToString() + "</color><size=70%> /" + totalPages.ToString();
        pageCounter.transform.GetChild(0).GetComponent<TextMeshPro>().text = (currentIndex + 1).ToString() + "<size=70%> /" + totalPages.ToString();

        reticle.GetComponent<CAReticle>().EnableMech();

        arrows.gameObject.SetActive(true);
        if (currentIndex == 0)
            arrows.GetChild(0).gameObject.SetActive(false);
        else if (currentIndex == totalPages-1)
            arrows.GetChild(1).gameObject.SetActive(false);
        else
        {
            arrows.GetChild(0).gameObject.SetActive(true);
            arrows.GetChild(1).gameObject.SetActive(true);
        }
        yield break;
        
    }

    #region Presenting Answer
    /// <summary>
    /// Shoots the stock at the question panel, then plays the correct or incorrect animation depending on if the player was correct or not.
    /// </summary>
    /// <param name="coroutine">The animation to play.</param>
    IEnumerator PanelShootAnim(string coroutine)
    {
        allowInput = false;
        reticle.allowInput = false;
        //Create stock projectile
        Transform whitePanel = activePanel.transform.parent.GetChild(4);
        whitePanel.gameObject.SetActive(true);
        whitePanel.localPosition = new Vector3(0, 0, 0);
        whitePanel.localScale = new Vector2(0.8f, 0.8f);
        Vector3 targetPos = reticle.selectedPanel.transform.position;

        float speed = ((targetPos - whitePanel.position).magnitude)/0.33333f;
        //Shoot stock at panel
        while(whitePanel.position != targetPos)
        {
            whitePanel.position = Vector3.MoveTowards(whitePanel.position,targetPos, speed*Time.deltaTime);
            whitePanel.localScale = Vector3.MoveTowards(whitePanel.localScale,new Vector3(0.2f, 0.2f, 1), 2.7f*Time.deltaTime);
            yield return null;
        }
        whitePanel.gameObject.SetActive(false);

        //Play animation
        StartCoroutine(coroutine);
        yield break;
    }

    /// <summary>
    /// Plays the 'correct' animation. Sets solved stock to correct.
    /// </summary>
    IEnumerator CorrectAnim() 
    {
        anim.SetTrigger("Correct");
        PlaySFX(correct);
        yield return new WaitForSeconds(1.9f);
        allowInput = true;
        reticle.allowInput = true;
        //set current stock to solved
        Animator stockAnim = currentStock.GetComponent<Animator>();
        currentStock.remainingLocks = -2;
        stockAnim.SetInteger("State", 2);
        stockAnim.SetBool("Clicked", false);

        Animator displayAnim = activePanelParent.GetComponent<Animator>();
        displayAnim.SetInteger("State", 2);
        displayAnim.SetBool("Clicked", false);

        totalAnswered++;
        isSelected = false;

        //set panel and pagebar panel to solved ¬`!
        GameObject pageBarPanel = pagebar.transform.GetChild(currentIndex).GetChild(reticle.selectedPanel.transform.GetSiblingIndex() - 1).gameObject;
        pageBarPanel.GetComponent<SpriteMask>().frontSortingOrder = 6;
        pageBarPanel.transform.GetChild(4).GetComponent<SpriteRenderer>().sprite = checkMark;

        GameObject pagePanel = reticle.selectedPanel.gameObject;
        pagePanel.GetComponent<SpriteMask>().frontSortingOrder = 1; 
        pagePanel.transform.GetChild(4).GetComponent<SpriteRenderer>().sprite = checkMark;
        pagePanel.GetComponent<Animator>().SetInteger("Shown", -2);
        reticle.selectedPanel.gameObject.GetComponent<PolygonCollider2D>().enabled = false;

        //updates the remaining locks across each stock.
        foreach (Transform child in currentStock.transform.parent)
        {
            CAStock childStock = child.GetComponent<CAStock>();
            if (childStock.remainingLocks > 0)
            {
                childStock.remainingLocks--;

            }
        }

        if(totalQuestions == totalAnswered)
        {
            StartCoroutine(Conclusion());
        }
    }

    /// <summary>
    /// Plays 'incorrect' animation and makes the player take damage.
    /// </summary>
    IEnumerator IncorrectAnim()
    {
        anim.SetTrigger("Incorrect");
        PlaySFX(incorrect);
        yield return new WaitForSeconds(1.9f);
        allowInput = true;
        reticle.allowInput = true;
        TakeDamage(1);
        hearts.GetComponent<Animator>().SetTrigger("Damage");
        CheckHealth();
    }

    #endregion

    /// <summary>
    /// Wait for stock unlock animation, then set stock to unlock 
    /// </summary>
    IEnumerator StockUnlock()
    {
        allowInput = false;
        reticle.allowInput = false;
        PlaySFX(stockUnlocked);
        currentStock.gameObject.GetComponent<Animator>().SetTrigger("Unlocked");
        Animator displayAnim = activePanelParent.GetComponent<Animator>();
        activePanelText.text = "";
        displayAnim.SetInteger("State", 1);
        displayAnim.SetTrigger("Unlock");
        yield return new WaitForSeconds(0.5f);
        allowInput = true;
        reticle.allowInput = true;
        currentStock.selectable = true;
        //currentStock.gameObject.GetComponent<SpriteRenderer>().sprite = currentStock.sprite;
        currentStock.remainingLocks = -1;
        currentStock.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = eveBorders[0];
        activePanelText.text = currentStock.flavourText;
        activePanel.sprite = currentStock.sprite;
        activePanel.transform.localPosition = currentStock.activePanelPos;
        activePanel.transform.localScale = currentStock.activePanelScale;
        yield break;
    }
    public void StartingReset()
    {
        totalAnswered = 0;
    }
    public void ResetPanels()
    {
        
        List<QuestionPanel> panels = FindObjectsOfType<QuestionPanel>().ToList();

        for (int i = 0; i < panels.Count; i++)
        {
            QuestionPanel panel = panels[i];
            
            SpriteMask[] masks = panel.GetComponentsInChildren<SpriteMask>();
            foreach (SpriteMask mask in masks)
            {
                mask.frontSortingOrder = 12;
            }
            panel.transform.GetChild(4).GetComponent<SpriteRenderer>().sprite = questionMark;
            panel.GetComponent<Animator>().SetInteger("Shown", 0);
            panel.GetComponent<PolygonCollider2D>().enabled = true;
        }
    }
    public void ResetStocks()
    {
        List<CAStock> stocks = FindObjectsOfType<CAStock>().ToList();
        for(int i = 0; i < stocks.Count; i++)
        {
            CAStock stock = stocks[i];
            Animator stockAnim = stock.GetComponent<Animator>();
            stock.remainingLocks = -2;
            stockAnim.SetInteger("State", 0);
            stockAnim.SetBool("Clicked", false);
            stockAnim.SetBool("Hovered", false);
            stockAnim.Rebind();
        }
        currentStock = firstStock;
    }
    IEnumerator Conclusion()
    {
        allowInput = false;
        reticle.allowInput = allowInput;
        gameTimer.StopTimer();
        yield return new WaitForSeconds(menuFadeWaitTime);

        ansAnimator.Play(ansAnimName);
        ansCanvas.enabled = true;
        SoundManager.instance.PlayMusic(null);
        SoundManager.instance.PlaySFX(ansSound);
        SoundManager.instance.PlayVoiceLine(ansVO);
        yield return new WaitForSeconds(Time.deltaTime);
        yield return new WaitForSeconds(ansAnimator.GetCurrentAnimatorStateInfo(0).length);
        if (GlobalFade.instance != null)
            GlobalFade.instance.FadeTo(0.5f);
        yield return new WaitForSeconds(0.5f);
        EndMinigame();
        StartCoroutine(UnLoad());
        yield break;
    }
    IEnumerator UnLoad()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
        yield break;
    }
}