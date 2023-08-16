using System.Collections.Generic;
using Configs;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public struct SoundLists
    {
        public List<string> Voices;
        public List<string> Sfxs;
        public List<string> Musics;
        public List<string> Rtcps;
    }
    
    public class DialogueGraph : EditorWindow
    {
        private string _fileName = AaGraphConstants.DefaultFileName;

        private DialogueGraphView _graphView;
        private TextField _fileNameTextField;
        private AaReactive<LanguageOperation> _languageOperation;

        [MenuItem("Aa/Dialogue Graph")]
        public static void OpenDialogueGraphWindow()
        {
            var window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent(AaGraphConstants.DialogueGraph);
        }

        private void OnEnable()
        {
            ConstructGraph();
            GenerateToolBar();
        }

        private void ConstructGraph()
        {
            _languageOperation = new();
            
            var gameSet = Resources.Load<GameSet>("GameSet");
            var soundLists = new SoundLists
            {
                Voices = gameSet.VoicesSet.GetKeys(),
                Sfxs = gameSet.SfxsSet.GetKeys(),
                Musics = gameSet.MusicSwitchesKeys.GetKeys(),
                Rtcps = gameSet.RtpcKeys.GetKeys(),
            };
            
            _graphView = new DialogueGraphView(_languageOperation, soundLists)
            {
                name = AaGraphConstants.DialogueGraph,
            };

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolBar()
        {
            var toolbar = new Toolbar();

            toolbar.Add(new Label("| FileName:"));

            _fileNameTextField = new TextField();
            _fileNameTextField.SetValueWithoutNotify(_fileName);
            _fileNameTextField.MarkDirtyRepaint();
            _fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
            toolbar.Add(_fileNameTextField);

            var saveDataButton = new Button(() => SaveData())
            {
                text = AaGraphConstants.SaveData,
            };
            toolbar.Add(saveDataButton);

            toolbar.Add(new Label(AaGraphConstants.LineSpace));

            var loadDataButton = new Button(() => LoadData())
            {
                text = AaGraphConstants.LoadData,
            };
            toolbar.Add(loadDataButton);

            toolbar.Add(new Label(AaGraphConstants.LineSpace));
            
            var phraseCreateButton = new Button(() => _graphView.CreatePhraseNode())
            {
                text = AaGraphConstants.PhraseNode,
            };
            toolbar.Add(phraseCreateButton);

            var imagePhraseCreateButton = new Button(() => _graphView.CreateImagePhraseNode())
            {
                text = AaGraphConstants.ImagePhraseNode,
            };
            toolbar.Add(imagePhraseCreateButton);

            var choiceCreateButton = new Button(() => _graphView.CreateChoiceNode())
            {
                text = AaGraphConstants.ChoiceNode,
            };
            toolbar.Add(choiceCreateButton);

            var forkCreateButton = new Button(() => _graphView.CreateForkNode())
            {
                text = AaGraphConstants.ForkNode,
            };
            toolbar.Add(forkCreateButton);

            var countCreateButton = new Button(() => _graphView.CreateCountNode())
            {
                text = AaGraphConstants.CountNode,
            };
            toolbar.Add(countCreateButton);
            
            var eventCreateButton = new Button(() => _graphView.CreateEventNode())
            {
                text = AaGraphConstants.EventNode,
            };
            toolbar.Add(eventCreateButton);
            
            var endCreateButton = new Button(() => _graphView.CreateEndNode())
            {
                text = AaGraphConstants.EndNode,
            };
            toolbar.Add(endCreateButton);
            
            var newspaperCreateButton = new Button(() => _graphView.CreateNewspaperNode())
            {
                text = AaGraphConstants.NewspaperNode,
            };
            toolbar.Add(newspaperCreateButton);

            rootVisualElement.Add(toolbar);
        }

        private void SaveData()
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                EditorUtility.DisplayDialog("Invalid filename for Save", "Please, input valid filename", "ok");
                return;
            }

            var saveUtility = GraphSaveUtility.GetInstance(_graphView, _languageOperation);
            saveUtility.SaveGraph(_fileName);
        }

        private void LoadData()
        {
            var saveUtility = GraphSaveUtility.GetInstance(_graphView, _languageOperation);

            if (!saveUtility.LoadGraph(ref _fileName))
            {
                Debug.LogError($"[{this}] Some error while loading graph");
                return;
            }

            _fileNameTextField.SetValueWithoutNotify(_fileName);
            _fileNameTextField.MarkDirtyRepaint();
        }

        private void OnDisable()
        {
            if (_graphView != null)
                rootVisualElement.Remove(_graphView);
        }
    }
}