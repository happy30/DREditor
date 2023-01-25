//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using DREditor.Characters;
using DREditor.Dialogues;
using DREditor.TrialEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
namespace NSD
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "DREditor/Minigames/NSD", fileName = "NSD Asset")]
    public class NSDBuilder : TrialDialogue
    {
        // Builder for NSD
        public int TrialNumber = 0;
        public List<TruthBullet> TruthBulletsKind = new List<TruthBullet>();
        public List<TruthBullet> TruthBulletsNormal = new List<TruthBullet>();
        public List<TruthBullet> TruthBulletsMean = new List<TruthBullet>();
        public List<Panel> PanelGroup = new List<Panel>();
        //[EventRef] public string StartingMusic;
        public AudioClip StartingMusic;
        public List<int> BulletNums = new List<int>(); // Unity Editor
        public List<int> BulletNumsNormal = new List<int>(); // Unity Editor
        public List<int> BulletNumsMean = new List<int>(); // Unity Editor
        public float timerMinutes;
        public float timerSeconds;
        public TrialDialogue EndDialogue;
        public string[] BulletStrings()
        {
            string[] arr = new string[TruthBulletsKind.Count];
            for (int i = 0; i < TruthBulletsKind.Count; i++)
            {
                arr[i] = TruthBulletsKind[i].Title;
            }
            
            return arr;
        }
        public string[] LieBulletStrings()
        {
            string[] arr = new string[TruthBulletsKind.Count];
            int count = 0;
            foreach (TruthBullet tb in TruthBulletsKind)
            {
                arr[count] = tb.LieTitle;
                count += 1;
            }
            return arr;
        }
        [System.Serializable]
        public class Panel : TrialLine
        {
            // VFX + Camera Animations in base
            public float waitTime = 0;
            public bool Bleeds;
            public List<TrialLine> FuckUpLines = new List<TrialLine>();
            public string PanelText;
            public List<Phrase> PhraseGroup = new List<Phrase>();
            public string[] wordArray;
            public List<WhiteNoise> whiteNoises = new List<WhiteNoise>(); // only 9 for P:EG
        }

        public List<string> PhraseTypeStrings = new List<string>()
        {
            Phrase.PhraseType.NonHit.ToString(),Phrase.PhraseType.PartlyHit.ToString(),Phrase.PhraseType.OnlyHit.ToString()
        };
        public static Dictionary<int, Phrase.PhraseType> PhraseValues = new Dictionary<int, Phrase.PhraseType>()
        {
           {0, Phrase.PhraseType.NonHit },{1, Phrase.PhraseType.PartlyHit },{2, Phrase.PhraseType.OnlyHit }
        };

        [System.Serializable]
        public class Phrase
        {
            public enum PhraseType
            {
                [Description("NonHit")] NonHit,
                [Description("PartlyHit")] PartlyHit,
                [Description("OnlyHit")] OnlyHit
            }

            public PhraseType phraseType;
            public int phraseTypeNum; // Unity Editor

            public PhraseAnimation anim;
            //public List<WNAnim> clipAnim = new List<WNAnim>(); // For using the Anim Clip Importer

            public string phraseText;
            public int phraseStart;// Unity Editor
            public int wordCount;
            public float phraseSize = 5;

            // Options when partly hitable
            public string[] partlyHitableWordArray;
            public int partlyHitableWordStart;
            public int partlyHitableWordCount;
            public string partlyHitablePhraseText;
            public bool allExcept;

            public TrialDialogue fuckUpDialogue;
            public bool isAnswer;
            public bool isLieAnswer;
            public int answerBulletNum; // Unity Editor
            public TruthBullet answerBullet;
            public bool hasWhiteNoise;
            public bool doesBleed;
            public bool isConsent;
            public void InitializeCurves(Phrase phrase, AnimationClip clip)
            {
                foreach(PhraseAnimation.AnimData anim in phrase.anim.animations)
                {
                    anim.InitializeCurveType(anim,clip);
                }
            }
            public Path path = new Path();
            [Serializable]
            public struct Path
            {
                public Vector3 position;
                public Vector4 rotation;
                public Cinemachine.CinemachinePath.Waypoint[] points;
                public AnimationCurve speedCurve;
            }
        }

        [System.Serializable]
        public class PhraseAnimation
        {
            /* Fill out basic info first
             * First you pick a type of animation
             * Then you fill out the information/data for that AnimationType 
             * Button available saying "Generate Curves"
             * Generate a curve field for each value
             * Remove/Undo Curves
             */

            public List<AnimData> animations = new List<AnimData>();
            public Vector3 spawnPoint;
            public Vector4 spawnAngle;
            

            public enum AnimType : int
            {
                Transform, Rotation, Scale, Fade, CharacterByCharacter
            }

            public static List<string> AnimTypeStrings = new List<string>()
            {
                AnimType.Transform.ToString(), AnimType.Rotation.ToString(), AnimType.Scale.ToString(), AnimType.Fade.ToString()
                , AnimType.CharacterByCharacter.ToString()
            };

            public Dictionary<int, AnimType> GetAnimType = new Dictionary<int, AnimType>()
            {
               {(int)AnimType.Transform,AnimType.Transform},{(int)AnimType.Rotation,AnimType.Rotation}, {(int)AnimType.Scale,AnimType.Scale}
                , {(int) AnimType.Fade, AnimType.Fade}, {(int) AnimType.CharacterByCharacter, AnimType.CharacterByCharacter}
            };

            [System.Serializable]
            public class AnimData
            {
                public AnimType currentAnimType;
                public int animTypeNum;

                public List<AnimationCurve> animationCurves = new List<AnimationCurve>();
                public CBCAnim cBCAnim;
                public void InitializeCurveType(AnimData anim, AnimationClip clip)
                {
                    switch (anim.currentAnimType)
                    {
                        case AnimType.Transform:
                            anim.tStruct.Init(anim, clip);
                            break;

                        case AnimType.Rotation:
                            anim.rStruct.Init(anim, clip);
                            break;

                        case AnimType.Scale:
                            anim.sStruct.Init(anim, clip);
                            break;

                        case AnimType.Fade:
                            anim.fStruct.Init(anim, clip);
                            break;
                    }
                }

                public PhraseTransform tStruct;
                [System.Serializable]
                public struct PhraseTransform
                {
                    public Vector3 startPosition;
                    public Vector3 endPosition;
                    public float startTime;
                    public float duration;
                    public void Init(AnimData anim, AnimationClip clip)
                    {
                        clip.SetCurve("", typeof(Transform), "localPosition.x", anim.animationCurves[0]);
                        clip.SetCurve("", typeof(Transform), "localPosition.y", anim.animationCurves[1]);
                        clip.SetCurve("", typeof(Transform), "localPosition.z", anim.animationCurves[2]);
                    }
                }

                public PhraseRotation rStruct;
                [System.Serializable]
                public struct PhraseRotation
                {
                    public Vector4 startRotation;
                    public Vector4 endRotation;
                    public float startTime;
                    public float duration;
                    public void Init(AnimData anim, AnimationClip clip)
                    {
                        clip.SetCurve("", typeof(Transform), "localEulerAngles.x", anim.animationCurves[0]);
                        clip.SetCurve("", typeof(Transform), "localEulerAngles.y", anim.animationCurves[1]);
                        clip.SetCurve("", typeof(Transform), "localEulerAngles.z", anim.animationCurves[2]);
                        clip.SetCurve("", typeof(Transform), "rotation.w", anim.animationCurves[3]);
                    }
                }

                public PhraseScale sStruct;
                [System.Serializable]
                public struct PhraseScale
                {
                    public Vector3 startScale;
                    public Vector3 endScale;
                    public float startTime;
                    public float duration;
                    public void Init(AnimData anim, AnimationClip clip)
                    {
                        clip.SetCurve("", typeof(Transform), "localScale.x", anim.animationCurves[0]);
                        clip.SetCurve("", typeof(Transform), "localScale.y", anim.animationCurves[1]);
                        clip.SetCurve("", typeof(Transform), "localScale.z", anim.animationCurves[2]);
                    }
                }

                public PhraseFade fStruct;
                [System.Serializable]
                public struct PhraseFade
                {
                    public float startFade;
                    public float endFade;
                    public float startTime;
                    public float duration;
                    public void Init(AnimData anim, AnimationClip clip)
                    {
                        //clip.SetCurve("", typeof(TextMeshPro), "fontColor.a", anim.animationCurves[0]);
                        /* This Function is a dud because the above function doesn't work properly. 
                         * The code to take care of this functionality is in
                         * PhraseText.cs PlayFade(NSDBuilder.PhraseAnimation.AnimData data, AnimationCurve curve);
                         */
                    }
                }

            }// AnimData

        }// PhraseAnimation

        [Serializable]
        public class WhiteNoise
        {
            public string text;
            public float size = 3;
            public List<WNAnim> anim = new List<WNAnim>();
            public PhraseAnimation panim;
            public Phrase.Path path = new Phrase.Path();
            public void InitializeCurves(WhiteNoise wn, AnimationClip clip)
            {
                foreach (PhraseAnimation.AnimData anim in wn.panim.animations)
                {
                    anim.InitializeCurveType(anim, clip);
                }
            }
            //public CBCAnim cbcAnim;
        }
        [Serializable]
        public class WNAnim
        {
            public string path;
            public string propertyName;
            public AnimationCurve curve;
        }
        public TimerDiff[] times = new TimerDiff[3]
    {
        new TimerDiff(GameManager.Difficulty.Kind),new TimerDiff(GameManager.Difficulty.Normal),new TimerDiff(GameManager.Difficulty.Mean)
    };
        #region TimerDiff
        public void SetTimerBasedOnDifficulty(TrialTimer timer)
        {
            TimerDiff timed = GetTime();
            float time = (timed.min * 60) + timed.sec;
            timer.StartTimer(time);
        }
        TimerDiff GetTime()
        {
            return times.Where(n => n.difficulty == GameManager.instance.logicDifficulty).ElementAt(0);
        }
    }
    
    
    #endregion
}
/// <summary>
/// Timer Difficulty, found in NSDBuilder.cs
/// </summary>
[Serializable]
public class TimerDiff
{
    public TimerDiff(GameManager.Difficulty diff, float baseMin = 1)
    {
        difficulty = diff;
        min = baseMin;
    }
    public GameManager.Difficulty difficulty;
    public float min;
    public float sec;
}
