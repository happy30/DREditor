//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class UIOptionGroup : MonoBehaviour
{
    [SerializeField] List<Selectable> options = new List<Selectable>();
    [SerializeField] List<NavGroup> navGroups = new List<NavGroup>();
    [SerializeField] List<NavSelect> navSelects = new List<NavSelect>();
    [SerializeField] UIChangerList changerList = null;

    [HideInInspector] public Selectable lastOption = null;
    public UnityEvent Selection;
    public UnityEvent Deselection;
    bool setup = false;
    bool selected = false;
    public void SetupGroup()
    {
        UIHandler.instance.OnChange.AddListener(Eval);
        if (lastOption == null)
        {
            lastOption = options[0];
            //lastOption.Select();
            UIDisplayChanger op = lastOption.GetComponent<UIDisplayChanger>();
            
            if (op)
            {
                op.Select();
            }
        }
        setup = true;
    }
    
    public void UnSetupGroup()
    {
        UIHandler.instance.OnChange.RemoveListener(Eval);
        if (changerList)
            changerList.Deselect();
        UIDisplayChanger op = lastOption.GetComponent<UIDisplayChanger>();
        if (op)
        {
            op.Select();
        }
        setup = false;
    }
    public void KeepVisualOnGroupLeave()
    {
        if (!setup)
        {
            try
            {
                UIDisplayChanger op = lastOption.GetComponent<UIDisplayChanger>();
                if (op)
                {
                    op.Select();
                }
            }
            catch
            {
                //Debug.LogError("Did you forget to add SetUp Group to the Menu Group's Start Event?");
            }
            
        }
        
    }
    public void Eval()
    {
        Check();
    }
    public bool Check()
    {
        GameObject g = EventSystem.current.currentSelectedGameObject;
        if (g == null)
        {
            Debug.LogWarning("The Current Selected object was null");
        }
        foreach (Selectable o in options)
        {
            if (g == o.gameObject)
            {
                if (!selected)
                {
                    selected = true;
                    Selection?.Invoke();
                }
                RewireNavigation(o);
                if (changerList)
                    changerList.Select();
                //nextGroup.ChangeGroupNav(lastOption);
                return true;
            }
        }
        if (selected)
        {
            selected = false;
            Deselection?.Invoke();
        }
        UIDisplayOption op = lastOption.GetComponent<UIDisplayOption>();
        if (op)
        {
            op.InvokeSelect();
        }
        if (changerList)
            changerList.Deselect();
        return false;
    }
    public void RewireNavigation(Selectable o)
    {
        lastOption = o;
        if (navGroups.Count != 0)
        {
            foreach (NavGroup nav in navGroups)
            {
                try
                {
                    nav.group.ChangeGroupNav(lastOption, nav.direction);
                }
                catch(Exception e)
                {
                    Debug.LogError(e.Message);
                    Debug.LogError(gameObject.name + " this happened on");
                }
            }
        }
        if (navSelects.Count != 0)
        {
            foreach (NavSelect nav in navSelects)
            {
                ChangeNavSelects(nav.selectable, lastOption, nav.direction);
            }
        }
    }
    /// <summary>
    /// Changes the navigation of the selectables on the group
    /// </summary>
    /// <param name="o"></param>
    public void ChangeGroupNav(Selectable o, Direction direction)
    {
        //Debug.LogWarning("Rewired Groups");
        switch (direction)
        {
            case Direction.Up:
                foreach (Selectable option in options)
                {
                    UIHelper.SelectOnUp(option, o);
                }
                break;
            case Direction.Down:
                foreach (Selectable option in options)
                {
                    UIHelper.SelectOnDown(option, o);
                }
                break;
            case Direction.Left:

                break;
            case Direction.Right:

                break;
        }
        
    }
    void ChangeNavSelects(Selectable on, Selectable to, Direction direction) // Overload for selectables
    {
        // put in checker? If list is empty
        // foreach in the list 
        //Debug.LogWarning("Rewired Selects");
        switch (direction)
        {
            case Direction.Up:
                UIHelper.SelectOnUp(on, to);
                break;
            case Direction.Down:
                UIHelper.SelectOnDown(on, to);
                break;
            case Direction.Left:

                break;
            case Direction.Right:

                break;
        }
    }
    public enum Direction
    {
        Up, Down, Left, Right
    }
    [Serializable]
    public class NavGroup
    {
        public UIOptionGroup group;
        [Header("Change the above objects Navigation for On")]
        public Direction direction;
    }
    [Serializable]
    public class NavSelect
    {
        public Selectable selectable;
        [Header("Change the above objects Navigation for On")]
        public Direction direction;
    }
}
