using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
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

            _graphView = new DialogueGraphView(_languageOperation)
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
                text = AaGraphConstants.NewPhraseNode,
            };
            toolbar.Add(phraseCreateButton);

            var choiceCreateButton = new Button(() => _graphView.CreateChoiceNode())
            {
                text = AaGraphConstants.NewChoiceNode,
            };
            toolbar.Add(choiceCreateButton);

            var forkCreateButton = new Button(() => _graphView.CreateForkNode())
            {
                text = AaGraphConstants.NewForkNode,
            };
            toolbar.Add(forkCreateButton);

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
                Debug.LogError("Some error while loading graph");
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