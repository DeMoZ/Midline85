using System.Collections;
using Configs;
using UnityEngine;

public abstract class AaGraphObjectEvent : MonoBehaviour
{
    protected bool IsAlive;
    protected ObjectEvents ObjectEvents;
    protected GameSet GameSet;
    

    public void SetCtx(ObjectEvents objectEvents, GameSet gameSet)
    {
        ObjectEvents = objectEvents;
        GameSet = gameSet;

        IsAlive = true;
    }

    public abstract IEnumerator AwaitInvoke();
    
    private void OnDestroy()
    {
        IsAlive = false;
    }
}