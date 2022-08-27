using System;
using UniRx;
using UnityEngine;

public class LevelScenePm : IDisposable
{
    public struct Ctx
    {
        
    }

    private Ctx _ctx;
    private CompositeDisposable _disposables;

    public LevelScenePm(Ctx ctx)
    {
        _ctx = ctx;

        CreateObjects();
        
        Debug.Log($"[{this}] constructor finished");
    }

    private void CreateObjects()
    {
       
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}