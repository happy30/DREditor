//Dialogue Event Script for DREditor by SeleniumSoul
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

namespace DREditor.Dialogues.Events
{
    public enum DialogueEventType
    {
        ChangePanelPositions, ChangeCharacterCameraFocus, ChangeMusic, Flash, Emotions, Custom
    }

    //The DialogueEvent class only houses the variables needed to fire off that event. A Dialogue Handler
    //is the one that invokes the event. This is the system that I'm going with for Distrust for now.
    //I know that this is not exactly clean, but it works. -SeleniumSoul
    [System.Serializable]
    public class DialogueEvent
    {
        public DialogueEventType type; //Checks what event is supposed to be invoked by the handler.

        //ChangePanelPositions Variables
        public string PosType;

        //ChangeCameraFocus Variables
        public int PanelWindow;
        public Vector3 CamPos = new Vector3(0f, 0f, 0f);

        //ChangeMusic Variables
        public int MusicNum;

        //Emotions - Temporarily Unused since the Emotions script is not included on this pull request
        //public Emotions Emotion;

        //Custom - Disabled on DREditor since it's unfinished
        //public SceneEvent CEvent;
        //Make the Dialogue Handler invoke a custom event.
    }

    // ===============================================================================================
    // Below here was the remains of a sad attempt of making a generic event system (Not UnityEvent). Will try to convert the dialogue event script to have a better architecture
    // when I'm better at coding, but for now, these are commented until I'm able to make one.
    //
    // If anyone is wondering what the hell am I going with this code, here's what I intended to do:
    // - The main abstract class should accept one or infinite type parameters of variables. (I'm convinced that the "infinite" part isn't possible actually. But hell, I'm looking for ways to implement it nonetheless)
    // ===============================================================================================

    // public abstract class DialogueEvent<T1,T2,T3,T4,T5>
    // {
    //     public DialogueEventType type;
    //     public T1 Value1;
    //     public T2 Value2;
    //     public T3 Value3;
    //     public T4 Value4;
    //     public T5 Value5;
    // }

    // public class ChangePanelPositions : DialogueEvent<string>
    // {
    //     //Should return this variable:
    //     //string PosType;
    // }

    // public class ChangeCameraFocus : DialogueEvent<int, Vector3>
    // {
    //     //Should return these variables:
    //     //int PanelWindow;
    //     //Vector3 CamPos = new Vector3(0f, 0f, 0f);
    // }

    // public class ChangeMusic: DialogueEvent<int>
    // {
    //     //Should return this variable:
    //     //int MusicNum;
    // }
}