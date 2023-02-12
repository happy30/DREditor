using UnityEngine;
using DREditor.Dialogues;
using UnityEngine.SceneManagement;

public class UIDialogueSubmanager : MonoBehaviour
{
    public GameObject DialogueBox;
    public GameObject DialogueBox_CG;

    private DialoguePlayer _diaplayer;

    private void Start()
    {
        _diaplayer = GameObject.FindGameObjectWithTag("System").GetComponent<DialoguePlayer>();

        _diaplayer.SayBoxManager = this;
    }

    public void DialogueBoxActive(bool ena)
    {
        DialogueBox.SetActive(ena);
    }
    public void DialogueBoxCGActive(bool ena)
    {
        DialogueBox_CG.SetActive(ena);
    }
}
