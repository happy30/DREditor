using System.Text.RegularExpressions;
using UnityEngine;
using DREditor.Dialogues;

public class DRBacklog : MonoBehaviour
{
    public GameObject LinePanel;
    public GameObject ScrollContent;
    public Backlog BL_File;

    public AudioClip Select;
    private AudioSource _AudSource;

    void Awake()
    {
        if (!_AudSource) _AudSource = GetComponent<AudioSource>();
        gameObject.GetComponentInParent<UIMenuSubmanager>().BacklogUI = gameObject;
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        InitialScroll();
    }

    void OnDisable()
    {
        DePopulateScroll();
    }

    void InitialScroll()
    {
        foreach (BacklogLines bck in BL_File.Lines)
        {
            PopulateScroll(bck);
        }
    }

    void PopulateScroll(BacklogLines bck)
    {
        GameObject pnl;
        DRLogPanel lpnl;
        string text;
        pnl = Instantiate(LinePanel);
        pnl.transform.SetParent(ScrollContent.transform, false);
        lpnl = pnl.GetComponent<DRLogPanel>();

        text = bck.Text;
        text = Regex.Replace(text, "<emp>", "<material=\"Nunito_Emphasis\">");
        text = Regex.Replace(text, "</emp>", "</material>");
        text = Regex.Replace(text, "<nar>", "<material=\"Nunito_Narration\">");
        text = Regex.Replace(text, "</nar>", "</material>");
        text = Regex.Replace(text, "<tut>", "<material=\"Nunito_Tutorial\">");
        text = Regex.Replace(text, "</tut>", "</material>");

        //Pass Line to Textfields
        if (bck.Charfile)
        {
            lpnl.NameField.text = bck.Charfile.FirstName + " " + bck.Charfile.LastName;

            if (bck.Charfile.Headshot != null) lpnl.Portait.texture = bck.Charfile.Headshot;
            else lpnl.Portait.color = new Color(0, 0, 0, 0);
        }
        else
        {
            lpnl.NameField.text = "";
            lpnl.NamePanel.color = new Color(100, 100, 100, 1);
            lpnl.Portait.color = new Color(0, 0, 0, 0);
        }

        lpnl.TextField.text = text;

        pnl.SetActive(true);
    }

    void DePopulateScroll()
    {
        Transform[] children = ScrollContent.GetComponentsInChildren<Transform>();

        for (int i = 1; i <= children.Length - 1; i++)
        {
            GameObject.Destroy(children[i].gameObject);
        }
    }

    void OnApplicationQuit()
    {
        BL_File.Lines.Clear();
    }

    public void PlaySFX()
    {
        _AudSource.PlayOneShot(Select);
    }
}
