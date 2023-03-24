using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class PhraseNode : AaNode
    {
        private string _personTxt;
        private string _phraseSketchTxt;

        public string PhraseSketchText => _phraseSketchTxt;

        public PhraseNode(PhraseNodeData data, List<string> languages, string guid)
        {
            Guid = guid;

            titleContainer.Add(new NodeTitleErrorField());

            var contentFolder = new Foldout();
            contentFolder.value = false;
            extensionContainer.Add(contentFolder);
            contentFolder.AddToClassList("aa-PhraseNode_extension-container");

            var titleTextField = new PhraseSketchField(data.PhraseSketchText, val =>
            {
                _phraseSketchTxt = val;
                title = GetTitle(_personTxt, _phraseSketchTxt);
            });
            contentFolder.Add(titleTextField);

            var personVisual = new PersonVisual(data.PersonVisualData, val =>
            {
                _personTxt = val;
                title = GetTitle(_personTxt, _phraseSketchTxt);
            });
            contentFolder.Add(personVisual);

            var phraseVisual = new PhraseVisual(data.PhraseVisualData);
            contentFolder.Add(phraseVisual);

            var phraseEvents = new PhraseEvents(data.EventVisualData, CheckNodeContent);
            contentFolder.Add(phraseEvents);

            var phraseContainer = new PhraseElementsTable();
            var phraseAssetsLabel = new Label("Phrase Assets");
            phraseAssetsLabel.AddToClassList("aa-BlackText");
            phraseContainer.Add(phraseAssetsLabel);

            for (var i = 0; i < languages.Count; i++)
            {
                var clips = data.PhraseSounds;
                var clip = clips != null && clips.Count > i
                    ? NodeUtils.GetObjectByPath<AudioClip>(clips[i])
                    : null;

                var phrase = data.Phrases != null && data.Phrases.Count > i
                    ? NodeUtils.GetObjectByPath<Phrase>(data.Phrases[i])
                    : null;
                
                phraseContainer.Add(new PhraseElementsRowField(languages[i], clip, phrase, CheckNodeContent));
            }

            phraseContainer.contentContainer.AddToClassList("aa-PhraseAsset_content-container");

            contentFolder.Add(phraseContainer);

            var inPort = GraphElements.GeneratePort(this, Direction.Input, Port.Capacity.Multi);
            inPort.portName = AaGraphConstants.InPortName;
            inputContainer.Add(inPort);

            var outPort = GraphElements.GeneratePort(this, Direction.Output, Port.Capacity.Multi);
            outPort.portName = AaGraphConstants.OutPortName;
            outputContainer.Add(outPort);

            RefreshExpandedState();
            RefreshPorts();
            CheckNodeContent();
        }

        public void CheckNodeContent()
        {
            var phrases = contentContainer.Query<PhraseAssetField>().ToList();
            var sounds = contentContainer.Query<PhraseSoundField>().ToList();
            var events = contentContainer.Query<EventAssetField>().ToList();

            var errorFields = new StringBuilder();

            if (phrases.Any(p => p.GetPhrase() == null))
                errorFields.Append("p.");

            if (sounds.Any(s => s.GetPhraseSound() == null))
                errorFields.Append("s.");

            if (events.Any(evt => evt.GetEvent() == null))
                errorFields.Append("e.");

            if (errorFields.Length > 0)
            {
                errorFields.Insert(0, "<color=red>");
                errorFields.Append("</color>");
            }

            titleContainer.Q<NodeTitleErrorField>().Label.text = errorFields.ToString();
        }

        public List<AudioClip> GetPhraseSounds() =>
            contentContainer.Query<PhraseSoundField>().ToList().Select(field => field.GetPhraseSound()).ToList();

        public List<Phrase> GetPhrases() =>
            contentContainer.Query<PhraseAssetField>().ToList().Select(field => field.GetPhrase()).ToList();

        public PersonVisual GetPersonVisual() =>
            contentContainer.Q<PersonVisual>();

        public PhraseVisual GetPhraseVisual() =>
            contentContainer.Q<PhraseVisual>();

        public List<EventVisual> GetEventsVisual() =>
            contentContainer.Query<EventVisual>().ToList();

        private string GetTitle(string person, string text)
        {
            return $"{person}\n{text}";
        }
    }
}