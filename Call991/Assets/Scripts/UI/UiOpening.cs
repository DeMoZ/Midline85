using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace UI
{
    public class UiOpening : AaWindow
    {
        private enum OpeningState
        {
            Logo = 0,
            Warning,
            PlayButton,
        }

        public struct Ctx
        {
            public ReactiveCommand OnClickStartGame;
            public Blocker Blocker;
            public CursorSet CursorSettings;
            public WwiseAudio AudioManager;
            public GameLevelsService GameLevelsService;
        }

        [Space] [SerializeField] private OpeningTimeSettings timeSettings = default;
        [Space] [SerializeField] private GameObject openingUi1 = default;
        [SerializeField] private GameObject openingUi2 = default;
        [SerializeField] private GameObject openingUi3 = default;
        [SerializeField] private Button startBtn = default;
        [SerializeField] private GameObject startTxt = default;
        [SerializeField] private ClickAnyButton anyButton = default;
        [SerializeField] private ProgressBar progressBar;

        [Space] [SerializeField] private AK.Wwise.Switch sceneMusic = default;

        private Ctx _ctx;
        private OpeningState _openingState = 0;
        private Coroutine _openingRoutine;
        private CancellationTokenSource _tokenSource;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _ctx.AudioManager.PlayMusic(sceneMusic);
            startBtn.onClick.AddListener(OnClickStart);
            anyButton.OnClick += OnClickStart;

            openingUi1.SetActive(false);
            openingUi2.SetActive(false);
            openingUi3.SetActive(false);

            _openingRoutine = StartCoroutine(OpeningRoutine());
        }

        public void OnSkip()
        {
            if (_openingState == OpeningState.PlayButton)
                return;

            if (_openingRoutine != null) StopCoroutine(_openingRoutine);

            _ctx.Blocker.EnableScreenFade(true);
            _openingState++;

            if ((int)_openingState < Enum.GetNames(typeof(OpeningState)).Length)
                _openingRoutine = StartCoroutine(OpeningRoutine());
        }

        private IEnumerator OpeningRoutine()
        {
            openingUi1.SetActive(_openingState == OpeningState.Logo);
            openingUi2.SetActive(_openingState == OpeningState.Warning);
            openingUi3.SetActive(_openingState == OpeningState.PlayButton);

            _ctx.Blocker.FadeScreenBlocker(false, timeSettings.fadeInTime).Forget();
            yield return new WaitForSeconds(timeSettings.fadeInTime);

            switch (_openingState)
            {
                case OpeningState.Logo:
                    yield return new WaitForSeconds(timeSettings.logoHoldTime);
                    break;
                case OpeningState.Warning:
                    yield return new WaitForSeconds(timeSettings.warningHoldTime);
                    break;
                case OpeningState.PlayButton:
                    progressBar.Set(0, "");
                    PressToPlayScreen().Forget();
                    yield break;
            }

            _ctx.Blocker.FadeScreenBlocker(true, timeSettings.fadeOutTime).Forget();
            yield return new WaitForSeconds(timeSettings.fadeOutTime);

            _openingState++;
            _openingRoutine = StartCoroutine(OpeningRoutine());
        }

        private async Task PressToPlayScreen()
        {
            startTxt.SetActive(false);
            startBtn.gameObject.SetActive(false);
            progressBar.gameObject.SetActive(true);

            _tokenSource = new CancellationTokenSource();
            var levels = _ctx.GameLevelsService.GetLevels();
            var currentProgress = 0f;
            for (var i = 0; i < levels.Count; i++)
            {
                var levelId = levels[i].EntryNodeData.LevelId;
                var levelData = await _ctx.GameLevelsService.AddressableDownloader.DownloadAsync<UnityEngine.Object>
                (levelId,
                    progress =>
                    {
                        currentProgress = (i + progress) / levels.Count;
                        progressBar.Set(currentProgress, levelId);
                    },
                    _tokenSource.Token);

                Addressables.Release(levelData);
            }

            startBtn.gameObject.SetActive(true);
            startTxt.SetActive(true);
            progressBar.gameObject.SetActive(false);
            _ctx.CursorSettings.ApplyCursor();
            _ctx.CursorSettings.EnableCursor(true);
        }

        public void OnClickStart()
        {
            OnClickStartAsync().Forget();
        }

        private async Task OnClickStartAsync()
        {
            _ctx.Blocker.FadeScreenBlocker(true, timeSettings.fadeOutTime).Forget();
            await Task.Delay((int)(timeSettings.fadeOutTime * 1000));

            _ctx.OnClickStartGame.Execute();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _tokenSource?.Token.ThrowIfCancellationRequested();
            startBtn.onClick.RemoveAllListeners();
            anyButton.OnClick -= OnClickStart;
        }
    }

    [Serializable]
    public class OpeningTimeSettings
    {
        public float fadeInTime = 1f;
        public float fadeOutTime = 2f;
        [Title("Opening Logo")] public float logoHoldTime = 3f;
        [Title("Opening Warning")] public float warningHoldTime = 5f;
    }
}