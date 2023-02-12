//StartScene

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;

public enum SceneType
{
	PointAndClick,
	FirstPerson,
	ClassTrial
}

/// <summary>
/// Assistant Directors are per scene/map.
/// </summary>
public class AssistantDirector : MonoBehaviour
{
	static public AssistantDirector current;
	static public bool PlayingAnimation;
	public SceneType SceneType;
	public bool Rereadable;
	public List<Dialogue> _DialogueFile;

	private bool _readall;
	private int _threadNum = 0;

	private void Awake()
	{
		if (current == null)
		{
			current = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private IEnumerator Start()
	{
		yield return new WaitForEndOfFrame();
		if (SceneType == SceneType.PointAndClick) DialogueEventSystem.StartListening("ShowMap", PlayDirector);
		if (_DialogueFile.Count != 0) PassDialogue();

		ScreenSetup();
	}

	private void OnDestroy()
	{
		DialogueEventSystem.StopListening("ShowMap", PlayDirector);
	}

	private void ScreenSetup()
	{
		switch (SceneType)
		{
			case SceneType.PointAndClick:
				DRScreenManager.current.SetGroupActive(1, false);
				DRScreenManager.current.SetGroupActive(2, false);
				DRScreenManager.current.SetGroupActive(3, false);
				DialoguePlayer.current.DRCursor = DRScreenManager.current.PointAndClickCursor;
				break;
			case SceneType.FirstPerson:
				DRScreenManager.current.SetGroupActive(1, true);
				DRScreenManager.current.SetGroupActive(2, true);
				DRScreenManager.current.SetGroupActive(3, true);
				DialoguePlayer.current.DRCursor = DRScreenManager.current.FirstPersonCursor;
				break;
			default:
				DRScreenManager.current.SetGroupActive(1, true);
				DRScreenManager.current.SetGroupActive(2, true);
				DRScreenManager.current.SetGroupActive(3, true);

				DialoguePlayer.current.DRCursor = DRScreenManager.current.FirstPersonCursor;
				break;
		}
	}

	public void PlayDirector(object unused = null)
	{
		SetControlActive(false);
		GetComponent<PlayableDirector>().Play();
	}

	public void PassDialogue()
	{
		if (_DialogueFile.Count != 0 || _DialogueFile[0] != null)
		{
			if (Rereadable)
			{
				DialogueHandler.DialogueAsset = ThreadPass();
				DialogueEventSystem.TriggerEvent("StartDialogue");
			}

			else
			{
				if (!_readall)
				{
					DialogueHandler.DialogueAsset = ThreadPass();
					DialogueEventSystem.TriggerEvent("StartDialogue");
				}
			}
		}
	}
	private Dialogue ThreadPass()
	{
		if (_threadNum < _DialogueFile.Count - 1)
		{
			_threadNum++;
			return _DialogueFile[_threadNum - 1];
		}
		else if (_threadNum >= _DialogueFile.Count)
		{
			_threadNum = _DialogueFile.Count;
			_readall = true;
			return _DialogueFile[_threadNum];
		}
		else
		{
			_readall = true;
			return _DialogueFile[_threadNum];
		}
	}

	public void SetControlActive(bool active)
	{
		DRScreenManager.current.SetGroupActive(1, active);
		DRScreenManager.current.SetGroupActive(2, active);
		DRScreenManager.current.SetGroupActive(3, active);
	}
}
