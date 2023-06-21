using System.Collections;
using I2.Loc;
using UnityEngine;

public class LevelTitle : PhraseObjectEvent
{
    [SerializeField] private LocalizedString chapterKey;
    [SerializeField] private LocalizedString titleKey;

    public override IEnumerator AwaitInvoke()
    {
        if (!ObjectEvents.EventsGroup.SkipTitle)
        {
            var keys = new string[]
            {
                chapterKey,
                titleKey
            };

            ObjectEvents.EventsGroup.OnShowTitle?.Execute((true, keys));
            ObjectEvents.EventsGroup.OnScreenFade?.Execute((false, GameSet.shortFadeTime));

            yield return new WaitForSeconds(GameSet.levelIntroDelay);

            ObjectEvents.EventsGroup.OnScreenFade?.Execute((true, GameSet.shortFadeTime));
            yield return new WaitForSeconds(GameSet.shortFadeTime);
            ObjectEvents.EventsGroup.OnShowTitle?.Execute((false, keys));
        }

        Destroy(gameObject);
    }
}