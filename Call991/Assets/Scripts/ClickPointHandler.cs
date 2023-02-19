using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class ClickPointHandler: IDisposable
{
    private const int Amount = 5;
    
    private readonly GameObject _pooledObject;
    private readonly List<GameObject> _pool = new ();
    private readonly Transform _clicksParent;

    public ClickPointHandler(GameObject pooledObject, Transform clicksParent)
    {
        _pooledObject = pooledObject;
        _clicksParent = clicksParent;
        
        for (var i = 0; i < Amount; i++)
        {
            _pool.Add(InstantiateObject(_pooledObject));
        }

        AaSelectable.OnMouseClickSelectable += OnClick;
    }

    private GameObject InstantiateObject(GameObject pooledImage)
    {
        var img = Object.Instantiate(pooledImage, _clicksParent);
        img.gameObject.SetActive(false);
        return img;
    }

    public void Dispose()
    {
        AaSelectable.OnMouseClickSelectable -= OnClick;
    }

    private GameObject GetPooledObject()
    {
        for(var i = 0; i < Amount; i++)
        {
            if(!_pool[i].activeInHierarchy)
                return _pool[i];
        }

        var img = InstantiateObject(_pooledObject);
        _pool.Add(img);
        return img;
    }
    
    private void OnClick(Vector2 position, Sprite sprite)
    {
        var img = GetPooledObject().GetComponent<Image>();
        img.sprite = sprite;
        img.transform.position = position;
        img.gameObject.SetActive(true);
    }
}