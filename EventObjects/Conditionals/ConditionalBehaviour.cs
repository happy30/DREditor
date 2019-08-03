using System;
using UnityEngine;

public interface IConditional
{
    bool Resolve();
}

public abstract class ConditionalBehaviour : MonoBehaviour, IConditional
{
    public abstract bool Resolve();

    /// <summary>
    /// Override this for Conditional classes that need to be initialized.
    /// </summary>
    /// <returns></returns>
    public virtual bool IsReady()
    {
        return true;
    }

    public abstract string ToLogicString(LogicNotation notation);

}

[Serializable]
public class ConditionalBehaviourInput
{
    public ConditionalBehaviour Input;
    public bool Invert;

    public bool Resolve()
    {
        if (Input == null) return !Invert;
        return Invert ^ Input.Resolve();
    }

    public bool IsReady()
    {
        if (Input == null) { return true; }
        else return Input.IsReady();
    }

    public string ToLogicString(LogicNotation notation)
    {
        string logicstring = Invert ? "If NOT: { " : "If: { ";
        if (Input != null) { logicstring += Input.ToLogicString(notation); }
        logicstring += " }";
        return logicstring;
    }
}


public class LogicNotation
{
    public static LogicNotation CSharp = new LogicNotation("(", ")", "!", " & ", " | ");
    public static LogicNotation Formal = new LogicNotation("(", ")", "¬", "∧", "∨");
    public static LogicNotation Current = CSharp;

    public readonly string BRACK_LEFT;
    public readonly string BRACK_RIGHT;
    public readonly string NOT;
    public readonly string AND;
    public readonly string OR;
    public readonly string XOR;

    public LogicNotation(string leftbracket, string rightbracket, string not, string and, string or)
    {
        BRACK_LEFT = leftbracket;
        BRACK_RIGHT = rightbracket;
        NOT = not;
        AND = and;
        OR = or;
    }
}