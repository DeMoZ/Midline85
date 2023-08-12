using System;
using UI;
using UniRx;
using UnityEngine;

public class DialogueService : IDisposable
{
    private CompositeDisposable _disposables;
    
    public readonly ReactiveCommand<UiPhraseData> OnShowPhrase;
    public readonly ReactiveCommand<UiImagePhraseData> OnShowImagePhrase;
    public readonly ReactiveCommand<UiPhraseData> OnHidePhrase;
    public readonly ReactiveCommand<UiImagePhraseData> OnHideImagePhrase;
    
    public readonly ReactiveCommand<(Container<bool> btnPressed, Sprite sprite)> OnShowNewspaper;
    public readonly ReactiveCommand OnShowLevelUi;
    public readonly ReactiveCommand OnSkipPhrase;

    public DialogueService()
    {
        _disposables = new CompositeDisposable();
        
        OnShowPhrase = new ReactiveCommand<UiPhraseData>().AddTo(_disposables);
        OnShowImagePhrase = new ReactiveCommand<UiImagePhraseData>().AddTo(_disposables);
        OnHidePhrase = new ReactiveCommand<UiPhraseData>().AddTo(_disposables);
        OnHideImagePhrase = new ReactiveCommand<UiImagePhraseData>().AddTo(_disposables);
        
        OnShowNewspaper = new ReactiveCommand<(Container<bool> btnPressed, Sprite sprite)>().AddTo(_disposables);
        OnShowLevelUi = new ReactiveCommand(); // on newspaper done
        OnSkipPhrase = new ReactiveCommand().AddTo(_disposables);
    }
    
    public void Dispose()
    {
        _disposables?.Dispose();
    }
}