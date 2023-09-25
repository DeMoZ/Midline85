using System;
using System.Collections;
using Core;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
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
        }

        [Space] [SerializeField] private OpeningTimeSettings timeSettings = default;
        [Space] [SerializeField] private GameObject openingUi1 = default;
        [SerializeField] private GameObject openingUi2 = default;
        [SerializeField] private GameObject openingUi3 = default;
        [SerializeField] private Button startBtn = default;
        [SerializeField] private ClickAnyButton anyButton = default;

        [Space] [SerializeField] private AK.Wwise.Switch sceneMusic = default;

        private Ctx _ctx;
        private OpeningState _openingState = 0;
        private Coroutine _openingRoutine;

        public void SetCtx(Ctx ctx)
        {
            _ctx = ctx;
            _ctx.AudioManager.PlayMusic(sceneMusic);
            startBtn.onClick.AddListener(OnClickStart);
            anyButton.OnClick += OnClickStart;

            openingUi1.gameObject.SetActive(false);
            openingUi2.gameObject.SetActive(false);
            openingUi3.gameObject.SetActive(false);
            
            _openingRoutine = StartCoroutine(OpeningRoutine());
        }
        
        public void OnSkip()
        {
            if (_openingRoutine != null) StopCoroutine(_openingRoutine);

            _ctx.Blocker.EnableScreenFade(true);
            _openingState++;

            if ((int)_openingState < Enum.GetNames(typeof(OpeningState)).Length)
                _openingRoutine = StartCoroutine(OpeningRoutine());
        }

        private IEnumerator OpeningRoutine()
        {
            openingUi1.gameObject.SetActive(_openingState == OpeningState.Logo);
            openingUi2.gameObject.SetActive(_openingState == OpeningState.Warning);
            openingUi3.gameObject.SetActive(_openingState == OpeningState.PlayButton);
            
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
                default:
                    _ctx.CursorSettings.ApplyCursor();
                    _ctx.CursorSettings.EnableCursor(true);
                    yield break;
            }

            _ctx.Blocker.FadeScreenBlocker(true, timeSettings.fadeOutTime).Forget();
            yield return new WaitForSeconds(timeSettings.fadeOutTime);

            _openingState++;
            _openingRoutine = StartCoroutine(OpeningRoutine());
        }

        public void OnClickStart()
        {
            _ctx.OnClickStartGame.Execute();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

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