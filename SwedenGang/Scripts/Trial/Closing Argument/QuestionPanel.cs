using UnityEngine;
using TMPro;

public class QuestionPanel : MonoBehaviour
{
    public CAStock answer;
    public string questionText;
    [SerializeField] Animator anim;

    private void Start()
    {
        transform.GetChild(0).GetComponent<TextMeshPro>().text = questionText;
        if (transform.localPosition.y > 0.3f)
            anim.SetBool("direction", true);// puts the text box below the question panel shape
    }


    private void OnTriggerEnter2D(Collider2D collision)//animation control
    {
        anim.SetInteger("Shown", 1);
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(anim.GetInteger("Shown") != -1)
            anim.SetInteger("Shown", 0);
    }
}
