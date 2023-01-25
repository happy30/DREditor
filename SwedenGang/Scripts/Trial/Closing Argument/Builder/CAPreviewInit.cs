using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CAPreviewInit : CAInitialiser
{
    [SerializeField] List<Sprite> currEveSprs = new List<Sprite>(); //list of borders for the activePanel
    [SerializeField] List<Sprite> eveBorders = new List<Sprite>(); //list of borders for stock panels
    public void InitArgument(ArgumentBuilder AB,int currentPage,int stockIndex) 
    {
        manager.enabled = false;
        foreach (ArgumentBuilder.Page pageData in AB.pages)//initialises each page
        {
            InitPage(pageData);
        }

        ClearEmptyStock();
        manager.currentStock = stockParent.transform.GetChild(stockIndex - 1).GetComponent<CAStock>();
        //Inititalisation of gameplay before the first frame
        manager.totalTime = AB.totalTime;
        manager.totalPages = totalPages;
        manager.totalQuestions = totalStock;

        //Setup for initial stock
        manager.currentStock.gameObject.transform.GetChild(1).gameObject.SetActive(true);
        manager.activePanelParent.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = currEveSprs[0];
        if (manager.currentStock.remainingLocks <= 0)
        {

            manager.activePanel.sprite = manager.currentStock.sprite;
            manager.activePanel.transform.localScale = manager.currentStock.activePanelScale;
            manager.activePanel.transform.localPosition = manager.currentStock.activePanelPos;
            manager.activePanelText.text = manager.currentStock.flavourText;

        }
        else
        {
            if (manager.currentStock.remainingLocks == 1)
                manager.activePanelText.text = "You need to solve another panel to unlock this panel";
            else
                manager.activePanelText.text = "You need to solve another " + manager.currentStock.remainingLocks.ToString() + " panels to unlock this panel";
        }

        //Gives the pages and the pagebar the correct rotation
        manager.pagebar.transform.rotation = Quaternion.Euler(0, 0, -3.933726f);
        manager.pages.transform.rotation = Quaternion.Euler(0, 0, -3.9f);
        //The first page to be displayed is displayed prominently
        MovePages(currentPage);
        


        //Set up display for which page you're on
        manager.pageCounter.text = "<color=#E3D13E>" + 1.ToString() + "</color><size=70%> /" + totalPages.ToString();
        manager.pageCounter.transform.GetChild(0).GetComponent<TextMeshPro>().text = 1.ToString() + "<size=70%> /" + totalPages.ToString();

        //Set the end position for the pagebar and the pages for when movement occurs
        manager.barEndPos = manager.pagebar.transform.position;
        manager.pageEndPos = manager.pages.transform.position;
        if (currentPage == 1)
            manager.arrows.GetChild(0).gameObject.SetActive(false);
        else if (currentPage == totalPages - 1)
            manager.arrows.GetChild(1).gameObject.SetActive(false);
        else if (totalPages == 1)
            manager.arrows.gameObject.SetActive(false);
        else
        {
            manager.arrows.GetChild(0).gameObject.SetActive(true);
            manager.arrows.GetChild(1).gameObject.SetActive(true);
        }

        //Set timer to correct time and camera to correct position
        manager.timer.text = Mathf.FloorToInt(manager.totalTime / 60).ToString() + ":" + Mathf.FloorToInt(manager.totalTime % 60).ToString();

        manager.enabled = true;
        

    }

    protected override void InitPanel(ArgumentBuilder.Page.QstnPanel panelData, Transform page)
    {
        //Create panel visually
        GameObject qstnPanel = Instantiate(panelPrefab, page.position, Quaternion.identity, page);
        qstnPanel.transform.position += new Vector3(panelData.panelPos.x, panelData.panelPos.y, 0) * 3 / 5; //Set position of panel. page scale = 0.8, pages parent object scale = 0.75, 0.75*0.8= 3/5. The numbers are outdated but u get the gist.
        qstnPanel.GetComponent<SpriteMask>().sprite = panelData.panelShape;

        //Set position of question mark
        Transform symbol = qstnPanel.transform.GetChild(4);
        symbol.localPosition = panelData.symbolOffset;
        symbol.localScale = new Vector2(symbol.localScale.x * panelData.symbolScale.x, symbol.localScale.y * panelData.symbolScale.y);

        //Set data in panel component
        QuestionPanel panelComp = qstnPanel.GetComponent<QuestionPanel>();
        panelComp.questionText = panelData.questionText;
        panelComp.answer = InitStock(panelData);

        for (int i = 0; i < qstnPanel.transform.childCount -1; i++)
        {
            qstnPanel.transform.GetChild(i).gameObject.SetActive(false);
        }
        
        totalStock++;
    }

    protected override CAStock InitStock(ArgumentBuilder.Page.QstnPanel panelData)
    {
        //Create stock
        CAStock eve = stockParent.transform.GetChild(totalStock).GetComponent<CAStock>();
        eve.sprite = panelData.panelShape; //Sprite to be displayed by activePanel when the stock is hovered

        //Setup stock sprite
        Transform spriteObj = eve.transform.GetChild(4);
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
            eve.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = eveBorders[0];
        }
        else
        {
            eve.selectable = false;
            eve.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = eveBorders[1];
        }

        return eve;
    }

    protected override void ClearEmptyStock()
    {
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
            eve.gameObject.SetActive(false);
        }
    }

    protected void MovePages(int pages) 
    {
        Vector3 pagesPosition = manager.pages.transform.position;
        pagesPosition = new Vector3(pagesPosition.x +  - 9.91f * (pages-1), pagesPosition.y + 0.63f * (pages - 1), 0);
        manager.pages.transform.position = pagesPosition;
        Vector3 barPosition = manager.pagebar.transform.position;
        barPosition = new Vector3(barPosition.x + -2.8f * (pages - 1), barPosition.y + 0.2f * (pages - 1), 0);
        manager.pagebar.transform.position = barPosition;

        if(pages != 1)
        {
            Transform currentPage = manager.pages.transform.GetChild(0);
            currentPage.transform.localScale = new Vector3(0.4f, 0.4f, 1);
            currentPage.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().color = new Vector4(0, 0, 0, 1);

        }

        Transform toBepage = manager.pages.transform.GetChild(pages - 1);
        toBepage.transform.localScale = new Vector3(0.65f, 0.65f, 1);
        toBepage.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().color = new Vector4(0, 0, 0, 0);
    }
}
