using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessHandler
{
    public bool start;
    public bool continuing;
    public bool end;
    
    public ProcessHandler()
    {
        start = false;
        continuing = false;
        end = false;
    }

    public void Status()
    {
        Debug.Log(start + " " + continuing + " " + end);
    }

    public void Start()
    {
        start = true;
        continuing = false;
        end = false;
    }

    public void Continue()
    {
        start = false;
        continuing = true;
        end = false;
    }

    public void End()
    {
        start = false;
        continuing = false;
        end = true;
    }

    public void Reset()
    {
        start = false;
        continuing = false;
        end = false;
    }
}
