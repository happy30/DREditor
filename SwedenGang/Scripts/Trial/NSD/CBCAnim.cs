//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using CharTween;
using DG.Tweening;
using UnityEditor;

namespace NSD
{
    /// <summary>
    /// This is for Character by Character Animation for NSD Text. 
    /// 
    /// To add a functional type of cbc animation from DOTween Do the following
    /// - Add this new type to the CBCType Enum
    /// - Create it's new Class. Ex: TransformData : AnimModuleData
    /// - Implement the new Class's specific information.
    /// - Add the Override Animate(int i, CharTweener tweener) Implementation (see other classes here for help)
    /// - Add implementation to the Switch statement in CastAnim()
    /// - Add Switch Implementation to ConfigType(CBCAnim.AnimModule module) in CBCAnimEditor.cs
    /// - Same as above but to DisplayAnim()
    /// - Create Editor function for new anim
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Minigames/CharbyChar Anim", fileName = "CBC Animation")]
    public class CBCAnim : ScriptableObject
    {
        [Tooltip("If the Text Starts invisible or not")]
        public bool isVisible = true;
        public Vector3 startPosition;
        public List<AnimModule> Anims = new List<AnimModule>();

        public enum CBCType
        {
            Transform, Rotation, Scale, Fade
        }
        internal NSDBuilder.Phrase phrase;
        public IEnumerator AnimateText(TMP_Text pText, NSDBuilder.Phrase phrase)
        {
            var text = pText;
            this.phrase = phrase;
            var tweener = text.GetCharTweener();
            // Turns the text invisible
            
            for (var i = 0; i < tweener.CharacterCount; ++i)
            {
                if (!isVisible)
                {
                    //pText.DOFade(0, 0);
                    //yield return new WaitForSeconds(0.1f);
                    FadeData f = new FadeData();
                    f.duration = 0;
                    f.ease = Ease.Linear;
                    f.phrase = phrase;
                    f.endValue = 0;
                    f.Animate(i, tweener);
                }

                /*var posTween = tweener.DOMove(i, startPosition, 0)
                    .SetLoops(0);
                posTween.fullPosition = OffsetTime(i, tweener.CharacterCount);*/

            }

            // Then does the animation(s)
            for (var i = 0; i < tweener.CharacterCount; ++i)
            {
                if (pText.text[i] == ' ')
                {
                    continue;
                }
                
                foreach(AnimModule module in Anims)
                {
                    CastAnim(module, i, tweener);
                    
                }
                yield return new WaitForSeconds(0.15f);

            }
            yield return null;
        }

        void CastAnim(AnimModule module, int i, CharTweener tweener)
        {
            switch (module.type)
            {
                case CBCType.Transform:
                    TransformData trans = (TransformData)module.data;
                    trans.Animate(i, tweener);
                    break;

                case CBCType.Rotation:
                    RotateData rot = (RotateData)module.data;
                    rot.Animate(i, tweener);
                    break;

                case CBCType.Scale:
                    ScaleData s = (ScaleData)module.data;
                    s.Animate(i, tweener);
                    break;

                case CBCType.Fade:
                    FadeData fade = (FadeData)module.data;
                    fade.phrase = phrase;
                    fade.Animate(i, tweener);
                    break;
            }
        }

        public static float OffsetTime(int i, int count)
        {
            //Debug.Log(Mathf.Lerp(0, 1, i / (float)(count - 1)));
            return Mathf.Lerp(0, 1, i / (float)(count - 1));
        }

        [System.Serializable]
        public class AnimModuleData
        {
            public float delay;
            public float duration = 1;
            public Ease ease;
            
            public virtual void Animate(int i, CharTweener tweener)
            {

            }
        }

        [System.Serializable]
        public class AnimModule
        {
            [SerializeReference] public AnimModuleData data;
            public CBCType type;
        }

        [System.Serializable]
        public class TransformData : AnimModuleData
        {
            public Vector3 endValue;

            public override void Animate(int i, CharTweener tweener)
            {
                Tweener transformTween = tweener.DOMove(i, endValue, duration)
                    .SetLoops(0)
                    .SetDelay(delay)
                    .SetEase(ease);

                transformTween.fullPosition = OffsetTime(i, tweener.CharacterCount);
            }
        }

        [System.Serializable]
        public class RotateData : AnimModuleData
        {
            public Vector3 endValue;
            public RotateMode mode;
            public override void Animate(int i, CharTweener tweener)
            {
                Tweener rotateTween = tweener.DORotate(i, endValue, duration, mode)
                    .SetLoops(0)
                    .SetDelay(delay)
                    .SetEase(ease);

                rotateTween.fullPosition = OffsetTime(i, tweener.CharacterCount);
            }
        }

        [System.Serializable]
        public class ScaleData : AnimModuleData
        {
            public float endValue;
            public override void Animate(int i, CharTweener tweener)
            {
                Tweener scaleTween = tweener.DOScale(i, endValue, duration)
                    .SetLoops(0)
                    .SetDelay(delay)
                    .SetEase(ease);

                scaleTween.fullPosition = OffsetTime(i, tweener.CharacterCount);
            }
        }

        [System.Serializable]
        public class FadeData : AnimModuleData
        {
            public float endValue = 1;
            public NSDBuilder.Phrase phrase;
            
            private bool IsHitableChar(int index)
            {
                string[] textArr = phrase.phraseText.Split(' ');
                int charIndex = 0;
                for(int i = 0; i < phrase.partlyHitableWordStart; i++)
                {
                    charIndex += textArr[i].Length;
                }

                if (index <= charIndex)
                    return false;

                for(int i = 0; i < phrase.partlyHitableWordArray.Length; i++)
                {
                    charIndex += phrase.partlyHitableWordArray[i].Length;
                }

                if (index <= charIndex)
                    return true;

                return false;
            }
            public override void Animate(int i, CharTweener tweener)
            {
                /* Reason for this code is because when we try to fade Character by Character
                 * The Gradient on the TMP Resets and partly hitable phrases look incorrect
                 */
                Color topLeft;
                Color topRight;
                Color botLeft;
                Color botRight;

                if(phrase.phraseType == NSDBuilder.Phrase.PhraseType.OnlyHit || 
                    (phrase.phraseType == NSDBuilder.Phrase.PhraseType.PartlyHit && IsHitableChar(i)))
                {
                    topLeft = new Color(0.99607843137f, 0.74901960784f, 0.17647058823f, endValue);
                    topRight = topLeft;
                    botLeft = new Color(0.99607843137f, 0.54901960784f, 0.11764705882f, endValue);
                    botRight = botLeft;
                }
                else
                {
                    topLeft = new Color(1, 1, 1, endValue);
                    topRight = topLeft;
                    botLeft = new Color(0.89803921568f, 0.89803921568f, 0.89803921568f, endValue);
                    botRight = new Color(0.72549019607f, 0.72549019607f, 0.72549019607f, endValue);
                }
                VertexGradient grad = new VertexGradient(topLeft,topRight,botLeft,botRight);
                var fadeTween = tweener.DOGradient(i, grad, duration)
                    .SetLoops(0)
                    .SetDelay(delay)
                    .SetEase(ease);

                fadeTween.fullPosition = OffsetTime(i, tweener.CharacterCount);
            }
        }

        
    }
}
