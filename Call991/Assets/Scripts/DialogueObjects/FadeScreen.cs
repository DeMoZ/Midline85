using System.Collections;
using UnityEngine;

public class FadeScreen : AaGraphObjectEvent
{
    [SerializeField] private bool showBlocker;

    public override IEnumerator AwaitInvoke()
    {
        ObjectEvents.EventsGroup.OnScreenFade?.Execute((showBlocker, GameSet.levelEndStatisticsUiFadeTime));
        yield return new WaitForSeconds(GameSet.levelEndStatisticsUiFadeTime);
        
        if (!IsAlive || gameObject == null) yield break;

        Destroy(gameObject);
    }
}