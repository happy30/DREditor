using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "DREditor/Minigames/Closing Argument",fileName ="Closing Agument Builder")]
public class ArgumentBuilder : MinigameBuilderBase
{
    public float totalTime;
    public List<Page> pages = new List<Page>();
    public int totalStock; //used when deleting unused stock for random stock ordering

    [System.Serializable]
    public class Page
    {
        public Sprite pageSprite;
        public List<QstnPanel> stock = new List<QstnPanel>();

        [System.Serializable]
        public class QstnPanel
        {
            public Sprite panelShape; 
            public Vector2 panelPos;
            public string questionText; //text displayed when hovering over the panel
            public Vector2 symbolOffset = new Vector2(0, 0); //offset of the symbol (question mark/ exclamation mark) from the center of the panel
            public Vector2 symbolScale = new Vector2(1, 1); //how big the symbol is

            public string flavourText; //Hint text when hovering over the stock
            public int noOfLocks = 0; //number of other stock that need to be completed in order for this one to be selected
            public Vector2 activeScale = new Vector2 (1,1); //scale of the active panel icon
            public Vector2 activePosition; //offset of the active panel icon from the centre.
            public Vector2 stockScale = new Vector2(1, 1); //as above but for the stock icon
            public Vector2 stockPosition;


        }

    }
}
