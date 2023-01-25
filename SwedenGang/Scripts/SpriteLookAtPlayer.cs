using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteLookAtPlayer : MonoBehaviour
{
    //Author : Redberd36, edited by Sweden/Zetsis
    [HideInInspector]
    public bool Lookon;
    private Transform target;
    //private GameObject PlayerCam;
    private void Awake()
    {
        GameManager.ModeChange += UpdateBillboard;
    }
    void Start()
    {
        if (GameObject.Find("Player"))
        {
            target = GameObject.Find("Main Camera").GetComponent<Transform>(); //Find the player object
            //PlayerCam = GameObject.FindGameObjectWithTag("MainCamera");
            //PlayerCam.GetComponent<FTEClick>().Dist_Val = 0;
            if (GameManager.instance.currentMode == GameManager.Mode.ThreeD)
                Lookon = true; //Player follow is on
        }
        
        
    }
    void FixedUpdate()
    {
        //Lookat/Rotate player function
        if (Lookon && !GameSaver.LoadingFile && !DialogueAnimConfig.instance.inDialogue.Value)
        {
            Vector3 direction = target.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);

            transform.rotation = rotation;
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);  //Limit Y and Z rotation
        }
    }
    void UpdateBillboard(GameManager.Mode mode)
    {
        if(mode == GameManager.Mode.TPFD)
        {
            Lookon = false;
            //transform.rotation = new Quaternion(0, 180, 0, 0);
        }
        else
        {
            Lookon = true;
        }
    }
    private void OnDestroy()
    {
        GameManager.ModeChange -= UpdateBillboard;
    }
    /*private void OnTriggerEnter(Collider Enter)
    {
        if (Enter.gameObject.name == "Main_Controller")
        {
            print("Entered personal space");
            PlayerCam.GetComponent<FTEClick>().Dist_Val = 2;
        }
    }
    private void OnTriggerExit(Collider Exit)
    {
        if (Exit.gameObject.name == "Main_Controller")
        {
            print("Exited personal space");
            PlayerCam.GetComponent<FTEClick>().Dist_Val = 0;
        }
    }*/
}
