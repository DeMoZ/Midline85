using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Test.Dialogues
{
    public class DialogueGraph : EditorWindow
    {
        private string _fileName = "NewDialogue";

        private DialogueGraphView _graphView;
        private TextField _fileNameTextField;
        private AaReactive<LanguageOperation> _languageOperation;

        [MenuItem("Aa/Dialogue Graph")]
        public static void OpenDialogueGraphWindow()
        {
            var window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent("Dialogue Graph");
        }

        private void OnEnable()
        {
            ConstructGraph();
            GenerateToolBar();
        }

        private void ConstructGraph()
        {
            _languageOperation = new ();

            _graphView = new DialogueGraphView(_languageOperation)
            {
                name = "Dialogue Graph",
            };

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolBar()
        {
            var toolbar = new Toolbar();

            toolbar.Add(new Label("| FileName:"));

            _fileNameTextField = new TextField(/*"FileName"*/);
            _fileNameTextField.SetValueWithoutNotify(_fileName);
            _fileNameTextField.MarkDirtyRepaint();
            _fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
            toolbar.Add(_fileNameTextField);
            
            var saveDataButton = new Button(() => SaveData())
            {
                text = "Save Data"
            }; 
            toolbar.Add(saveDataButton);

            toolbar.Add(new Label(" | "));

            var loadDataButton = new Button(() => LoadData())
            {
                text = "Load Data"
            }; 
            toolbar.Add(loadDataButton);

            toolbar.Add(new Label(" | "));
            
            var phraseCreateButton = new Button(() => _graphView.CreatePhraseNode("New Phrase"))
            {
                text = "New Phrase"
            };
            toolbar.Add(phraseCreateButton);
            
            var choiceCreateButton = new Button(() => _graphView.CreateChoiceNode("New Choice"))
            {
                text = "New Choice"
            };
            toolbar.Add(choiceCreateButton);

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
            rootVisualElement.Remove(_graphView);
        }
    }
}