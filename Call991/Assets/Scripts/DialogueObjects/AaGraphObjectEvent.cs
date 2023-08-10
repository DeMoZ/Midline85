using System.Collections;
using Configs;
using UnityEngine;

public abstract class AaGraphObjectEvent : MonoBehaviour
{
    protected ObjectEvents ObjectEvents;
    protected GameSet GameSet;
    
    public void SetCtx(ObjectEvents objectEvents, GameSet gameSet)
    {
        ObjectEvents = objectEvents;
        GameSet = gameSet;
    }
    
    public abstract IEnumerator AwaitInvoke();
}