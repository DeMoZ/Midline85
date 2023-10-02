using System.Collections;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using UnityEngine;

public class LevelWarning : AaGraphObjectEvent
{
    [SerializeField] private LocalizedString[] keys;

    public override IEnumerator AwaitInvoke()
    {
        if (!ObjectEvents.EventsGroup.SkipWarning)
        {
            var lines = new string [keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                lines[i] = keys[i];
            }
            
            yield return new WaitForSeconds(1);
            
            ObjectEvents.EventsGroup.OnScreenFade?.Execute((false, GameSet.shortFadeTime));
            yield return new WaitForSeconds(GameSet.shortFadeTime);

            var linesTime = lines.Length * GameSet.levelWarningLineDelay + lines.Length * GameSet.levelWarningLineFadeTime;
            var time = linesTime > GameSet.levelWarningTotalDelay ? linesTime : GameSet.levelWarningTotalDelay;
            ObjectEvents.EventsGroup.OnShowWarning?.Execute((true, lines, GameSet.levelWarningLineDelay, GameSet.levelWarningLineFadeTime));
            yield return new WaitForSeconds(time);

            ObjectEvents.EventsGroup.OnScreenFade?.Execute((true, GameSet.shortFadeTime));
            yield return new WaitForSeconds(GameSet.shortFadeTime);
            ObjectEvents.EventsGroup.OnShowWarning?.Execute((false, lines, GameSet.levelWarningLineDelay, GameSet.levelWarningLineFadeTime));
        }

        Destroy(gameObject);
    }
}