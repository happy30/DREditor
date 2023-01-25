//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using CharTween;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using NSD;
using DREditor.Camera;
using System.Reflection;
using UnityEditor;
using Cinemachine;
using DREditor.Dialogues;

public class PhraseText : MonoBehaviour
{
    [HideInInspector] public TMP_Text pText;
    [HideInInspector] public Animation phraseAnim;
    [HideInInspector] public NSDBuilder.Phrase textPhrase;
    [Tooltip("This is shows the name of the Hitable Text Gradient Object")]
    public string hitGradientName = "\"HitText\"";
    public string consentGradientName = "\"ConsentText\"";
    DRTrialCamera cam;
    BoxCollider hitBox;
    BoxCollider mainBox => (BoxCollider)GetComponents(typeof(BoxCollider))[1];
    List<BoxCollider> addedBoxes = new List<BoxCollider>();

    private void Awake()
    {
        pText = GetComponent<TMP_Text>();
        phraseAnim = GetComponent<Animation>();
        cam = FindObjectOfType<DRTrialCamera>();
        hitBox = GetComponent<BoxCollider>();
    }
    public void InitializeText(NSDBuilder.Phrase phrase)
    {
        textPhrase = phrase;
        //if (textPhrase.isAnswer)
            //Debug.Log("Answer Bullet: " + phrase.answerBullet.Title);
        gameObject.transform.localScale = Vector3.one;
        /*
         * Below makes it to where using the dolly track only works once 
         */
        
        if (phrase.anim.spawnPoint == Vector3.zero || phrase.anim.spawnPoint == null)
        {
            //Debug.Log("Set Position");
            gameObject.transform.position = phrase.anim.spawnPoint; // Make local postion? + Local position bool in builder?
        }
        else
        {
            //Debug.Log("Set Local");
            gameObject.transform.localPosition = phrase.anim.spawnPoint;
        }
        
        Vector4 rot = phrase.anim.spawnAngle;
        gameObject.transform.localEulerAngles = new Vector3(rot.x, rot.y, rot.z);
        pText.text = phrase.phraseText;
        pText.fontSize = phrase.phraseSize;
        // orient text near space of the camera
        transform.parent.rotation = new Quaternion(0, cam.transform.rotation.y, 0, cam.transform.rotation.w);
        transform.parent.position = new Vector3(0, cam.transform.position.y, 0);
        InitializeHit(phrase);
    }

    private void InitializeHit(NSDBuilder.Phrase phrase)
    {
        string gradientName = phrase.isConsent ? consentGradientName : hitGradientName;
        if (phrase.phraseType == NSDBuilder.Phrase.PhraseType.OnlyHit)
        {
            hitBox.enabled = true;
            pText.text = "<gradient=" + gradientName + ">" + pText.text + "</gradient>";
            pText.autoSizeTextContainer = true;
            pText.ForceMeshUpdate();
            ClearHitBoxes();
        }
        else if (phrase.phraseType == NSDBuilder.Phrase.PhraseType.PartlyHit)
        {
            hitBox.enabled = true;
            string[] arr = pText.text.Split(' ');
            arr[phrase.partlyHitableWordStart] = "<gradient=" + gradientName + ">" + arr[phrase.partlyHitableWordStart];
            arr[phrase.partlyHitableWordStart + phrase.partlyHitableWordCount-1] += "</gradient>";
            pText.text = string.Join(" ", arr);
            pText.autoSizeTextContainer = true;
            pText.ForceMeshUpdate();
            ClearHitBoxes();
        }
        else if (phrase.phraseType == NSDBuilder.Phrase.PhraseType.NonHit)
        {
            hitBox.enabled = false;
            pText.colorGradientPreset = null;
            pText.autoSizeTextContainer = true;
            pText.ForceMeshUpdate();
            ClearHitBoxes();
        }

        StartCoroutine(SetHitBox(phrase));
    }
    GameObject pathRef;
    /// <summary>
    /// This is for Animating with the Dolly Track on Cinemachine
    /// </summary>
    /// <param name="currentPhrase"></param>
    public void StartAnimation(NSDBuilder.Phrase currentPhrase)
    {
        // Try catch loop to set a bool for safety
        
        try
        {
            if (currentPhrase.path.points.Length > 0)
            {
                //Debug.Log("Wasn't Null");
                TestCart cart;
                GameObject par = gameObject.transform.parent.gameObject;
                TestCart c = par.GetComponent<TestCart>();
                if (c != null)
                {
                    cart = c;
                    c.m_Path = null;
                    c.m_Speed = 0;
                    c.m_Position = 0;
                }
                else
                    cart = par.AddComponent<TestCart>();
                cart.m_Position = 0;
                cart.m_PositionUnits = CinemachinePathBase.PositionUnits.Normalized;
                pathRef = new GameObject();
                GameObject ch = new GameObject();
                ch.transform.parent = pathRef.transform;
                CinemachinePath p = ch.AddComponent<CinemachinePath>();

                p.gameObject.transform.localPosition = currentPhrase.path.position;
                Vector4 rot = currentPhrase.path.rotation;
                p.gameObject.transform.localEulerAngles = new Vector3(rot.x, rot.y, rot.z);

                pathRef.transform.rotation = new Quaternion(0, cam.transform.rotation.y, 0, cam.transform.rotation.w);
                pathRef.transform.position = new Vector3(0, cam.transform.position.y, 0);
                transform.localPosition = new Vector3(0, -1, 0);

                cart.curve = currentPhrase.path.speedCurve;
                cart.useCurve = true;

                p.m_Waypoints = currentPhrase.path.points;
                cart.m_Path = p;
                cart.m_Speed = 1;
                cart.enabled = true;
            }
        }
        catch
        {

        }
        float count = 0;
        foreach (NSDBuilder.PhraseAnimation.AnimData data in currentPhrase.anim.animations)
        {
            if (data.currentAnimType == NSDBuilder.PhraseAnimation.AnimType.Fade)
            {
                PlayFade(data, data.animationCurves[0]);
                count += data.fStruct.duration;
            }

            if (data.currentAnimType == NSDBuilder.PhraseAnimation.AnimType.CharacterByCharacter)
            {
                StartCoroutine(data.cBCAnim.AnimateText(pText, currentPhrase));
            }
        }
    }
    void ClearHitBoxes()
    {
        hitBox.center = Vector3.zero;
        if (addedBoxes.Count > 0)
        {
            foreach (BoxCollider box in addedBoxes)
            {
                Destroy(box);
            }
            addedBoxes.Clear();
        }
    }

    IEnumerator SetHitBox(NSDBuilder.Phrase phrase)
    {
        pText.ForceMeshUpdate();
        yield return new WaitForEndOfFrame();
        if (phrase.phraseType == NSDBuilder.Phrase.PhraseType.NonHit
            || phrase.phraseType == NSDBuilder.Phrase.PhraseType.OnlyHit)
        {
            hitBox.size = pText.bounds.size;
            mainBox.size = hitBox.size;
            yield break;
        }
        var textInfo = pText.textInfo;
        TMP_LineInfo[] lineInfo = textInfo.lineInfo;
        TMP_WordInfo[] wordInfos = textInfo.wordInfo;
        int wordInPhrase = 0;
        bool foundHitStart = false;
        bool foundHitEnd = false;

        int lineCount = textInfo.GetRealLineCount();

        

        bool hasOddLineBreaks = lineCount % 2 != 0;
        bool isMultiLine = lineCount > 1;
        bool foundCalled = false;
        bool carryOver = false;
          
        //Debug.Log("Line Length: " + lineInfo.Length);
        /*
        for (int i = 0; i < lineInfo.Length; i++)
        {
            Debug.Log("Line: " + i + " is: " + textInfo.GetLineText(i));
        }
        */
        
        //Debug.Log("Real Line Length is: " + textInfo.GetRealLineCount());
        for (int i = 0; i < lineInfo.Length; i++) // For each line
        {
            if (!textInfo.GetValidLineIndexes().Contains(i))
            {
                wordInPhrase += lineInfo[i].wordCount;
                continue;
            }

            //Debug.Log("Starting New Line: " + i);

            TMP_LineInfo line = lineInfo[i];
            

            float colliderWidth;

            BoxCollider collider = GetComponent<BoxCollider>();

            if (foundHitStart && !foundHitEnd) // if more than one hitable word past line break
            {
                //Debug.Log("Identified that Partly Hitable extends to another line");
                carryOver = true;
                collider = gameObject.AddComponent<BoxCollider>();
                addedBoxes.Add(collider);
            }

            
            float bottomLeft = 0;
            float bottomRight = 0;
            for (int j = 0; j < line.wordCount; j++) // For each word in the line
            {
                TMP_WordInfo word = wordInfos[wordInPhrase];

                if (wordInPhrase >= phrase.partlyHitableWordStart
                    && wordInPhrase < (phrase.partlyHitableWordStart + phrase.partlyHitableWordCount)) // Indicates we are on a hitable word
                {

                    if (j == 0 && foundHitStart && !foundHitEnd ||
                        wordInPhrase == phrase.partlyHitableWordStart)
                    {
                        bottomLeft = textInfo.characterInfo[word.firstCharacterIndex].bottomLeft.x;
                    }

                    if (wordInPhrase == phrase.partlyHitableWordStart) // Recognizes this word as the first word in the partly hitable
                    {
                        //Debug.Log("Found Hit Start: " + word.GetWord());
                        foundHitStart = true;
                    }

                    if (j == line.wordCount - 1 && foundHitStart && !foundHitEnd ||
                        wordInPhrase + 1 == (phrase.partlyHitableWordStart + phrase.partlyHitableWordCount))
                    {
                        bottomRight = textInfo.characterInfo[word.lastCharacterIndex].bottomRight.x;
                    }
                }

                wordInPhrase++;

                // Recognizes this word as the last word in the partly hitable
                if (wordInPhrase == (phrase.partlyHitableWordStart + phrase.partlyHitableWordCount)) 
                {
                    //Debug.Log("Found Hit End: " + word.GetWord());
                    foundHitEnd = true;
                }

            } // end of each word for

            if ((!foundCalled || carryOver) && foundHitStart)
            {
                //float yDist = 0;
                int yCenterRatio = 0;
                if (isMultiLine)
                {
                    if (hasOddLineBreaks)
                        yCenterRatio = (lineCount / 2) - i;
                    else
                    {
                        yCenterRatio = (lineCount / 2) - 1 - i;
                        /*
                        Debug.Log("Y center Calculation: " + (lineCount / 2) + " - " + (textInfo.GetRealIndexOf(i) + 1));
                        yCenterRatio = (lineCount / 2) - (textInfo.GetRealIndexOf(i) + 1);

                        if (textInfo.GetRealLineCount() == 2)
                        {
                            yDist += yDist == 0 ? line.lineHeight / 2 : line.lineHeight;
                        }
                        else
                        {
                            if (yCenterRatio < 0)
                            {
                                for (int x = textInfo.GetRealIndexOf(lineInfo.Length / 2) - 1; x <= i; x++)
                                {
                                    Debug.Log("x: " + x);
                                    yDist += yDist == 0 ? lineInfo[x].lineHeight / 2 : lineInfo[x].lineHeight;
                                    Debug.Log("Line Height " + lineInfo[x].lineHeight);
                                }
                            }
                            else
                            {
                                for (int x = textInfo.GetRealIndexOf(lineInfo.Length / 2) - 1; x > i; x--)
                                {
                                    Debug.Log("x: " + x);
                                    yDist += yDist == 0 ? lineInfo[x].lineHeight / 2 : lineInfo[x].lineHeight;
                                    Debug.Log("Line Height " + lineInfo[x].lineHeight);
                                }
                            }
                        }
                        */
                        
                        //if (yCenterRatio < 0)
                            //yDist *= -1;

                        //Debug.Log("After Calc: " + yCenterRatio);
                        //yCenterRatio -= 1;
                        //if (textInfo.GetRealIndexOf(i) < lineCount && yCenterRatio > 0)
                        //yCenterRatio *= -1;

                        if (yCenterRatio >= 0)
                        {
                            //Debug.Log("Y CENTER WAS ZERO");
                            yCenterRatio += 1;
                        }
                    }
                }
                //Debug.Log("Y Center Ratio: " + yCenterRatio);
                //Debug.Log("Found Hit Start If statement passed");
                foundCalled = true;
                colliderWidth = bottomRight - bottomLeft;
                float xCenter = bottomRight - bottomLeft;
                xCenter /= 2;
                xCenter += bottomLeft;
                float evenDist = yCenterRatio < 0 ? (line.lineHeight * yCenterRatio) + (line.lineHeight / 2)
                    : (line.lineHeight * yCenterRatio) - (line.lineHeight / 2);
                if (!hasOddLineBreaks)
                    collider.center = new Vector3(xCenter, evenDist, -0.09f); // even
                else
                    collider.center = new Vector3(xCenter, line.lineHeight * yCenterRatio, -0.09f);
                collider.size = new Vector3(colliderWidth , line.lineHeight, 0.2f);
            }
        }

        Vector3 size = pText.bounds.size;
        mainBox.size = size;
        //Debug.Log("End of Setting Phrase");
        yield break;
    }
    
    #region Fade Animation
    public void PlayFade(NSDBuilder.PhraseAnimation.AnimData data, AnimationCurve curve)
    {
        StartCoroutine(Fade(data, curve));
    }

    IEnumerator Fade(NSDBuilder.PhraseAnimation.AnimData data, AnimationCurve curve)
    {
        if(data.fStruct.startTime != 0)
        {
            yield return new WaitForSeconds(data.fStruct.startTime);
            pText.color = new Color(pText.color.r, pText.color.g, pText.color.b, data.fStruct.startFade);
        }
        else
        {
            pText.color = new Color(pText.color.r, pText.color.g, pText.color.b, data.fStruct.startFade);
        }
        
        if (data.fStruct.endFade != 0)
        {
            pText.DOFade(data.fStruct.endFade, data.fStruct.duration).SetEase(curve);
        }
        else
        {
            pText.DOFade(data.fStruct.endFade, data.fStruct.duration);
        }
        
        yield return null;
    }
    #endregion

    #region Interaction
    //bool isHovering = false;
    public void Check(Vector3 hitPoint) => StartCoroutine(CheckText(hitPoint));
    IEnumerator CheckText(Vector3 hitPoint)
    {
        // Check Bullet
        if ((textPhrase.isAnswer && NSDManager.instance.currentBullet == textPhrase.answerBullet)
            || (NSDManager.instance.lieMode && textPhrase.isLieAnswer && NSDManager.instance.currentBullet == textPhrase.answerBullet))
        {
            NSDReticle.ShowOrHide(); // To make sure it hides
            NSDManager.instance.canEffect = false;
            NSDManager.instance.RemoveSlow();
            GetComponent<TMProShatter>().SetMesh();
            ShatterText(hitPoint);
            SoundManager.instance.PlaySFX(NSDManager.instance.BulletHitSFX);
            NSDManager.instance.StopPanels(false);
            yield return new WaitForSeconds(1);
            float waitTime = NSDManager.instance.DisplayCounter(textPhrase.isConsent);
            SoundManager.instance.PlaySFX(NSDManager.instance.CounterSound);
            if (!textPhrase.isConsent)
                SoundManager.instance.PlayVoiceLine(NSDManager.instance.ProtagCounterSound);
            else
                SoundManager.instance.PlayVoiceLine(NSDManager.instance.ProtagConsentSound);

            //Debug.Log(waitTime);
            yield return new WaitForSeconds(waitTime);
            NSDManager.instance.counterTexture.enabled = false;
            NSDManager.instance.gameObject.GetComponentInChildren<UIBreak>().ShatterTheScreen();
            SoundManager.instance.PlaySFX(NSDManager.instance.BreakSound);
            yield return new WaitForSeconds(5);
            NSDManager.instance.StopNSD();
        }
        else
        {
            SoundManager.instance.PlaySFX(NSDManager.instance.HitIncorrectSFX);
            Debug.Log("You fucked up the shot! Current Bullet: " + NSDManager.instance.currentBullet.Title);
            if (textPhrase.isAnswer)
                Debug.Log("Answer Bullet: " + textPhrase.answerBullet.Title);
            // You Fucked up Dialogue instance plays
            TrialDialogue dia = textPhrase.fuckUpDialogue;
            if (textPhrase.fuckUpDialogue == null)
            {
                Debug.LogError("Hey Dum Dum! There's no Fuck Up Dialogue on this Phrase!");
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                NSDManager.instance.PhraseReset();
                NSDReticle.ShowOrHide();
                NSDManager.instance.StopPanels(true, dia);
            }
        }
        

        yield return null;
    }

    void ShatterText(Vector3 hitPoint)
    {
        Destroy(GetComponent<CharTweener>());
        phraseAnim.Stop();
        

        GetComponent<TMProShatter>().Shatter(hitPoint - transform.position);
    }
    private void OnMouseEnter()
    {
        //Debug.Log("Entered");
        //NSDReticle.SetHover();
    }

    private void OnMouseExit()
    {
        //Debug.Log("Exited");
        //NSDReticle.SetHover();
    }


    #endregion
    public void Reset()
    {
        GetComponent<TMProShatter>().ResetMesh();
        pText.text = "";
        hitBox.enabled = false;
        
        
        
        pText.autoSizeTextContainer = false;
        GameObject par = gameObject.transform.parent.gameObject;
        TestCart c = par.GetComponent<TestCart>();
        if (c != null)
        {
            Destroy(c);
        }
        if (pathRef != null)
            Destroy(pathRef.gameObject);
        transform.position = new Vector3(0, 0, 0);
        //transform.parent.position = new Vector3(0, 0, 0);
    }
}
public static class Exten
{
    public static string GetLineText(this TMP_TextInfo textInfo, int index)
    {
        string lineText = "";
        TMP_LineInfo[] lineInfo = textInfo.lineInfo;
        TMP_WordInfo[] wordInfos = textInfo.wordInfo;
        int wordInPhrase = 0;
        

        
        
        for (int i = 0; i < lineInfo.Length; i++) // For each line
        {
            TMP_LineInfo line = lineInfo[i];
            for (int j = 0; j < line.wordCount; j++) // For each word in the line
            {
                TMP_WordInfo word = wordInfos[wordInPhrase];
                if (i == index)
                {
                    lineText += word.GetWord();
                }
                wordInPhrase++;
            }
        }
        
        return lineText;
    }
    public static int GetRealLineCount(this TMP_TextInfo textInfo)
    {
        int realLineCount = 0;

        for (int i = 0; i < textInfo.lineInfo.Length; i++) // For each line
            if (textInfo.GetLineText(i) != "")
                realLineCount++;

        return realLineCount;
    }
    public static List<int> GetValidLineIndexes(this TMP_TextInfo textInfo)
    {
        List<int> list = new List<int>();

        for (int i = 0; i < textInfo.lineInfo.Length; i++) // For each line
            if (textInfo.GetLineText(i) != "")
                list.Add(i);

        return list;
    }
    public static int GetRealIndexOf(this TMP_TextInfo textInfo, int index)
    {
        return textInfo.GetValidLineIndexes().IndexOf(index);
    }
}
