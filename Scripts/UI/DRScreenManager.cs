//DR Screen Manager by SeleniumSoul for DREditor
//March 2020

using UnityEngine;
using DREditor.Dialogues;

public class DRScreenManager : MonoBehaviour
{
	static public DRScreenManager current;
	public DRInput _controls;
	public GameObject[] UIGroups;
	public GameObject PointAndClickCursor, FirstPersonCursor;

	//Toggle Parameters
	private bool blcheck = false;
	private bool hlpcheck = false;
	private bool toggleUI = true;

	void Awake()
	{
		if (current == null)
		{
			current = this;
		}
		else
		{
			Destroy(gameObject);
		}

		if (toggleUI == true)
		{
			toggleUI = !toggleUI;
			UIOpacity(toggleUI);
		}
	}

	private void Start()
	{
		PointAndClickCursor.SetActive(false);
		FirstPersonCursor.SetActive(false);
	}
	
	void Update()
	{
		//if (Input.GetKeyDown(KeyCode.F2) && !hlpcheck)
		//{
		//    blcheck = !blcheck;
		//    InMenu.Value = blcheck;
		//    //if (blcheck) Time.timeScale = 0f;
		//    //else Time.timeScale = 1f;
		//    backlog?.SetActive(blcheck);
		//}

		//if (Input.GetKeyDown(KeyCode.F3) && !blcheck)
		//{
		//    hlpcheck = !hlpcheck;
		//    InMenu.Value = hlpcheck;
		//    Debug.Log(InMenu.Value);
		//    //if (hlpcheck) Time.timeScale = 0f;
		//    //else Time.timeScale = 1f;
		//    help?.SetActive(hlpcheck);
		//}

		if (Input.GetButtonDown("Fire2") && DialogueHandler.InDialog && !DialogueHandler.InMenu)
		{
			toggleUI = !toggleUI;
			UIOpacity(toggleUI);
		}

		else if (Input.anyKeyDown && toggleUI == false)
		{
			toggleUI = !toggleUI;
			UIOpacity(toggleUI);
		}
	}

	void OnHelp()
	{
		if (!blcheck)
		{
			hlpcheck = !hlpcheck;
			DialogueHandler.InMenu= hlpcheck;
			//if (hlpcheck) Time.timeScale = 0f;
			//else Time.timeScale = 1f;
			//help?.SetActive(hlpcheck);
		}
	}

	void OnBacklog()
	{
		if (hlpcheck)
		{
			blcheck = !blcheck;
			DialogueHandler.InMenu = blcheck;
			//if (blcheck) Time.timeScale = 0f;
			//else Time.timeScale = 1f;
			//backlog?.SetActive(blcheck);
		}
	}

	void OnCancel()
	{
		if (DialogueHandler.InDialog && !DialogueHandler.InMenu)
		{
			toggleUI = !toggleUI;
			UIOpacity(toggleUI);
		}
	}

	void UIOpacity(bool check)
	{
		//if (!check)
		//{
		//    foreach (CanvasGroup d in UI_Objects)
		//    {
		//        //d.alpha = 0;
		//    }
		//}
		//else
		//{
		//    foreach (CanvasGroup d in UI_Objects)
		//    {
		//        //d.alpha = 1;
		//    }
		//}
	}

	public void SetGroupActive(int index, bool active)
	{
		UIGroups[index].SetActive(active);
	}

	public void SetActiveAll(bool active)
	{
		foreach (GameObject ui in UIGroups)
		{
			ui.SetActive(active);
		}
	}
}
