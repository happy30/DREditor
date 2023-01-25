//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Dialogues;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using DREditor.PlayerInfo;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DialogueTextConfig : MonoBehaviour
{
    public static DialogueTextConfig instance = null;
    public TextMeshProUGUI dialogueBox = null;
    public TextMeshProUGUI cgBox = null;
    [SerializeField] Color tutorialColor = new Color(42, 113, 20, 255);
    [SerializeField] Color32 internalColor = new Color32(77, 193, 250, 255);
    PlayerInput playerInput;
    public bool isWaitingForClick = true;
    private bool skipText = false;
    private TextMeshProUGUI Box = null;
    public TextMeshProUGUI GetBox() => Box;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            this.enabled = false;
        Box = dialogueBox;
    }
    private void Start()
    {
        UIHandler.ToTitle += ResetTrialBox;
        playerInput = GameManager.instance.GetInput();
    }
    private void OnDisable()
    {
        UIHandler.ToTitle -= ResetTrialBox;
    }
    private TextMeshProUGUI TrialBox = null;
    public void SetTrialBox(TextMeshProUGUI box)
    {
        Box = box;
        TrialBox = box;
    }
    
    public void ResetTrialBox()
    {
        if (TrialBox != null)
            Box = dialogueBox;
    }
    public void SwapBox(bool to)
    {
        if (to)
        {
            Box = CGPlayer.instance.cgText;
        }
        else
        {
            Box = GameManager.instance.currentMode == GameManager.Mode.Trial && TrialBox != null ? TrialBox : dialogueBox;
        }
        
        /*
        if (GameManager.instance.currentMode == GameManager.Mode.Trial && TrialBox != null)
        {
            Box = Box == TrialBox ? CGPlayer.instance.cgText : TrialBox;
            Debug.LogWarning("Called Swap TrialDia to: " + Box.gameObject.name);
        }
        else
        {
            
            Box = Box == dialogueBox ? cgBox : dialogueBox;
            Debug.LogWarning("Called Swap Dia to: " + Box.gameObject.name);
        }
        */
    }
    public void DisplayText(Line line) => StartCoroutine(TextDisplay(line));
    IEnumerator TextDisplay(Line currentLine)
    {
        skipText = false;
        if (currentLine.Speaker != null && currentLine.Speaker.FirstName.Equals("Tutorial"))
            Box.color = tutorialColor;
        else
            Box.color = new Color32(255, 255, 255, 255);

        int c = currentLine.Text.Count(x => x == '@');
        for (int i = 0; i < c; i++)
        {
            if (i % 2 == 0)
                currentLine.Text = TextColorInator(currentLine.Text, '@', false);
            else
                currentLine.Text = TextColorInator(currentLine.Text, '@', true);
        }

        string text = currentLine.Text;
        Box.maxVisibleCharacters = Box.maxVisibleCharacters != 0 ? 0 : Box.maxVisibleCharacters;
        Box.text = text;
        string _notag = Regex.Replace(text, "<.*?>", string.Empty);
        Box.text = Box.text.Replace("\r", "");

        //string tag = "";
        //bool inTag = false;
        int sCount = 0;
        for (int i = 0; i < _notag.Length && !skipText; i++)
        {
            
            if (i >= 1 && !skipTextStarted)
            {
                StartCoroutine(SkipText());
            }
            else
            {
                //Debug.LogWarning("Did not call skip text!");
            }

            
            char letter = text[i];

            if (letter == '$')
            {
                Box.color = internalColor;
                Box.text = text.Substring(sCount+1);
                sCount += 1;
                
                yield return new WaitForSeconds(Time.deltaTime);
                continue;
            }

            if (letter == '^')
            {
                if (!DialogueAnimConfig.instance.protagUIIsShown)
                    DialogueAnimConfig.instance.ShowProtag();
                else
                    DialogueAnimConfig.instance.HideProtag();
                Box.text = text.Substring(sCount + 1);
                sCount += 1;
                yield return new WaitForSeconds(Time.deltaTime);
                continue;
            }
            if (letter == '%')
            {
                if (DialogueAnimConfig.instance.protagUIIsShown)
                    DialogueAnimConfig.instance.HideProtag();
                Box.text = text.Substring(sCount + 1);
                sCount += 1;
                yield return new WaitForSeconds(Time.deltaTime);
                continue;
            }

            Box.maxVisibleCharacters = i;
            if (!DialogueAssetReader.instance.ffMode)
                yield return new WaitForSecondsRealtime(PlayerInfo.instance.settings.TextSpeed * 0.01f);

        }
        Box.maxVisibleCharacters += 1;
        if (skipText)
        {
            int count = SymbolCount(currentLine.Text);
            
            Box.text = text.Substring(count);
            Box.text = Box.text.Replace("\r", "");
            Box.maxVisibleCharacters = Box.text.Length;
        }
        
        isWaitingForClick = false;

        yield return null;
    }
    private readonly List<string> symbols = new List<string>()
    {
        "$", "%", "^"
    };
    private int SymbolCount(string text)
    {
        int count = 0;
        for(int i = 0; i < symbols.Count; i++)
        {
            string symbol = symbols[i];
            if (text.Contains(symbol))
                count++;
        }
        return count;
    }

    #region Trial Overload
    public void DisplayText(TrialLine line) => StartCoroutine(TextDisplay(line));
    IEnumerator TextDisplay(TrialLine currentLine)
    {
        skipText = false;
        if (currentLine.Speaker != null && currentLine.Speaker.FirstName.Equals("Tutorial"))
            Box.color = tutorialColor;
        else
            Box.color = new Color32(255, 255, 255, 255);

        int c = currentLine.Text.Count(x => x == '@');
        for (int i = 0; i < c; i++)
        {
            if (i % 2 == 0)
                currentLine.Text = TextColorInator(currentLine.Text, '@', false);
            else
                currentLine.Text = TextColorInator(currentLine.Text, '@', true);
        }

        //Box.text = currentLine.Text;
        //Canvas.ForceUpdateCanvases();
        //string text = GetFormattedText(Box, currentLine.Text);
        //Box.text = string.Empty;
        string text = currentLine.Text;
        Box.maxVisibleCharacters = Box.maxVisibleCharacters != 0 ? 0 : Box.maxVisibleCharacters;
        Box.text = text;
        string _notag = Regex.Replace(text, "<.*?>", string.Empty);


        //string tag = "";
        //bool inTag = false;
        int sCount = 0;
        for (int i = 0; i < _notag.Length && !skipText; i++)
        {

            if (i >= 1 && !skipTextStarted)
                StartCoroutine(SkipText());


            char letter = text[i];

            if (letter == '$')
            {
                Box.color = new Color32(77, 193, 250, 255);
                Box.text = text.Substring(sCount + 1);
                sCount += 1;

                yield return new WaitForSeconds(Time.deltaTime);
                continue;
            }
            Box.maxVisibleCharacters = i;
            if (!TrialDialogueManager.instance.ffMode)
                yield return new WaitForSeconds(PlayerInfo.instance.settings.TextSpeed * 0.01f);

        }
        Box.maxVisibleCharacters += 1;
        if (skipText)
        {
            int count = SymbolCount(currentLine.Text);

            Box.text = text.Substring(count);
            Box.maxVisibleCharacters = Box.text.Length;
        }

        isWaitingForClick = false;

        yield return null;
    }
    #endregion

    bool skipTextStarted = false;
    IEnumerator SkipText()
    {
        skipTextStarted = true;
        while (isWaitingForClick)
        {
            if (DialogueAssetReader.instance)
            {
                if (DialogueAssetReader.instance.ffMode)
                {
                    break;
                }
                
            }
            if (playerInput.actions["Submit"].triggered)
            {
                break;
            }
            
            yield return null;
        }
        skipTextStarted = false;
        skipText = true;
        yield return null;
    }
    public void ClearText()
    {
        Box.maxVisibleCharacters = 0;
        Box.text = "";
    }
    #region THE TEXT COLOR-INTATOR AHAHAHAHAHAA
    public string TextColorInator(string x, char character, bool isColor)
    {
        if (!isColor)
        {
            var regex = new Regex(Regex.Escape(character.ToString()));
            var newText = regex.Replace(x, "<gradient=\"HitText\">", 1);
            return newText;
        }
        else
        {
            var regex = new Regex(Regex.Escape(character.ToString()));
            var newText = regex.Replace(x, "</gradient>", 1);
            return newText;
        }
    }
    #endregion
}
