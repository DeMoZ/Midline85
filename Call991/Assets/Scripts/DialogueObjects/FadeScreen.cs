using System.Collections;
using UnityEngine;

public class FadeScreen : PhraseObjectEvent
{
    [SerializeField] private bool showBlocker;

    public override IEnumerator AwaitInvoke()
    {
        ObjectEvents.EventsGroup.OnScreenFade?.Execute((showBlocker, GameSet.levelEndStatisticsUiFadeTime));
        yield return new WaitForSeconds(GameSet.levelEndStatisticsUiFadeTime);
        Destroy(gameObject);
    }
}