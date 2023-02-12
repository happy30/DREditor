using UnityEngine;
using DREditor.Dialogues;

public class UICursorManager : MonoBehaviour
{
	public GameObject PointAndClickCursor, FirstPersonCursor;

	void Start()
	{
		switch (AssistantDirector.current.SceneType)
		{
			case SceneType.PointAndClick:
				DialoguePlayer.current.DRCursor = PointAndClickCursor;

				PointAndClickCursor.SetActive(true);
				FirstPersonCursor.SetActive(false);
				break;
			case SceneType.FirstPerson:
				DialoguePlayer.current.DRCursor = FirstPersonCursor;

				PointAndClickCursor.SetActive(false);
				FirstPersonCursor.SetActive(true);
				break;
			default:
				PointAndClickCursor.SetActive(true);
				FirstPersonCursor.SetActive(false);
				break;
		}
	}
}
