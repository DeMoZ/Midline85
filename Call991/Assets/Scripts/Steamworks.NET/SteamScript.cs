using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(message: "Steam initialized:" + SteamManager.Initialized);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
