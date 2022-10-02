using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Configs;
using Data;
using UniRx;
using UnityEngine;

namespace Core
{
    public class LevelEndPm : IDisposable
    {
        public struct Ctx
        {
            public GameSet gameSet;
            public PlayerProfile profile;
            public AchievementsSo achievementsSo;
            public string endLevelConfigsPath;
            public PhraseEventSoundLoader phraseEventSoundLoader;
            public PhraseEventVideoLoader phraseEventVideoLoader;
            public ReactiveCommand<float> onHideLevelUi;
            public ReactiveCommand<float> onShowStatisticUi;
            public ReactiveCommand<string> onPhraseLevelEndEvent;
            public ReactiveCommand<List<StatisticElement>> onPopulateStatistics;
        }

        private Ctx _ctx;
        private CompositeDisposable _disposables;

        public LevelEndPm(Ctx ctx)
        {
            _ctx = ctx;
            _disposables = new CompositeDisposable();

            _ctx.onPhraseLevelEndEvent.Subscribe(OnLevelEnd);
        }

        private async void OnLevelEnd(string eventId)
        {
            Debug.LogWarning($"[{this}] Level End endId = {eventId}");
            _ctx.onHideLevelUi.Execute(_ctx.gameSet.levelEndLevelUiDisappearTime);
            await Task.Delay((int) (_ctx.gameSet.levelEndLevelUiDisappearTime * 1000));
            Debug.LogWarning($"[{this}] on hide awaited");

            var soFile = Path.Combine(_ctx.endLevelConfigsPath, eventId);
            var conf = await ResourcesLoader.LoadAsync<PhraseEndLevelEventSo>(soFile);

            if (!string.IsNullOrWhiteSpace(conf.soundEventId))
                _ctx.phraseEventSoundLoader.LoadMusicEvent(conf.soundEventId);

            if(!string.IsNullOrWhiteSpace(conf.videoEventId))
                _ctx.phraseEventVideoLoader.LoadVideoEvent(conf.videoEventId);
            
            var statistics = GetStatistics();
    
            _ctx.onPopulateStatistics.Execute(statistics);
            _ctx.onShowStatisticUi.Execute(_ctx.gameSet.levelEndStatisticsUiFadeTime);
            await Task.Delay((int) (_ctx.gameSet.levelEndStatisticsUiFadeTime * 1000));
            // _ctx.onLevelEnd?.Execute(statistics);
        }

        private List<StatisticElement> GetStatistics()
        {
            var statisticElements = new List<StatisticElement>();
            var choices = _ctx.profile.GetPlayerData().choices;

            foreach (var achievement in _ctx.achievementsSo.achievements)
            {
                var isReceived = GetAchievementState(achievement.requirements, choices);

                statisticElements.Add(new StatisticElement
                {
                    sprite = achievement.sprite,
                    description = achievement.descriptionTopKey,
                    isReceived = isReceived,
                });
            }

            return statisticElements;
        }

        private bool GetAchievementState(Dictionary<string, bool> achievement, List<string> choices)
        {
            /*
            // TODO REMOVE, Test only
            choices.Clear();
            choices.AddRange(new List<string>
            {
                "c.word.034",
                "C.word.037",
                "c.word.055",
                "c.word.036"
            });
            //<-------
            */
            var result = true;
            if (achievement == null) return false;
            
            foreach (var pair in achievement)
            {
                var keys = pair.Key.Split('|');

                var containsOne = ContainsOneOfKey(keys, choices);
                if (pair.Value)
                {
                    if (!containsOne)
                    {
                        result = false;
                        break;
                    }
                }
                else
                {
                    if (containsOne)
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }

        private bool ContainsOneOfKey(string[] keys, List<string> choices) => 
            keys.Any(choices.Contains);

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}