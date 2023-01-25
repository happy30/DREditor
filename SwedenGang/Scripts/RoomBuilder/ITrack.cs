//Author: Benjamin "Sweden" Jillson : Sweden#6386 For Project Eden's Garden
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITrack
{
    public object Save();
    public void Load(object ob);
}

public interface IWritable
{
    public Type WriteType { get; set; }
    public string Write();

}

public class SaveChunk
{
    public string Type;
    public string Data;
}
