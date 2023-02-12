//Dialogue Event script by SeleniumSoul for DREditor.
//
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DREditor.Dialogues.Events
{
    public interface IDialogueEvent
	{
		void TriggerDialogueEvent();

		#if UNITY_EDITOR
		void EditorUI();
		void ToggleHelpBox();
		void ShowHelpBox();
		#endif
	}

    public class DiaEvent : UnityEvent<object>{ }

    static public class DialogueEventSystem
    {
        static public Dictionary<string, DiaEvent> _eventDict = new Dictionary<string, DiaEvent>();

		public static void StartListening(string eventName, UnityAction<object> listener)
		{
			if (_eventDict.TryGetValue(eventName, out DiaEvent thisEvent))
			{
				thisEvent.AddListener(listener);
			}
			else
			{
				thisEvent = new DiaEvent();
				thisEvent.AddListener(listener);
                _eventDict.Add(eventName, thisEvent);
			}
		}

		public static void StopListening(string eventName, UnityAction<object> listener)
		{
			if (_eventDict.TryGetValue(eventName, out DiaEvent thisEvent))
			{
				thisEvent.RemoveListener(listener);
			}
		}

		public static void TriggerEvent(string eventName)
		{
			if (_eventDict.TryGetValue(eventName, out DiaEvent thisEvent))
			{
				Debug.Log("DREditor <color=blue>(DialogueEventSystem)</color>: Triggering " + eventName + " event.");
				thisEvent.Invoke(null);
			}
			else
			{
				Debug.LogWarning($"DREditor (DialogueEventSystem): Tryna trigger { eventName }, but it somehow doesn't exist.\nYou need to have to use StartListening first before you invoke the event. This is ignorable if it's intended.");
			}
		}

		public static void TriggerEvent(string eventName, object value = null)
		{
			if (_eventDict.TryGetValue(eventName, out DiaEvent thisEvent))
			{
				Debug.Log($"DREditor <color=blue>(DialogueEventSystem)</color>: Triggering { eventName } event.");
				thisEvent.Invoke(value);
			}
			else
			{
				Debug.LogWarning($"DREditor (DialogueEventSystem): Tryna trigger { eventName }, but it somehow doesn't exist.\nYou need to have to use StartListening first before you invoke the event. This is ignorable if it's intended.");
			}
		}
	}
}