using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Test.Dialogues
{
    public class PhraseNode : AaNode
    {
        private readonly Vector2 _defaultNodeSize = new(150, 200);

        public string DialogueText;

        private string _personTxt;
        private string _phraseSketchTxt;
        
        public PhraseNode(PhraseNodeData nodeData, List<string> languages, string guid = null)
        {
            title = nodeData.DialogueText;
            DialogueText = nodeData.DialogueText;
            Guid = guid ?? System.Guid.NewGuid().ToString();

            var contentFolder = new Foldout();
            contentFolder.value = false;
            contentContainer.Add(contentFolder);
            
            var titleTextField = new PhraseSketchField(val =>
            {
                _phraseSketchTxt = val;
                title = GetTitle(_personTxt, _phraseSketchTxt);
            });
            contentFolder.Add(titleTextField);   
            
            var line0 = new Label("   Person");
            contentFolder.Add(line0);

            var personVisual = new PersonVisual(nodeData.PersonVisualData, val =>
            {
                _personTxt = val;
                title = GetTitle(_personTxt, _phraseSketchTxt);
            });
            contentFolder.Add(personVisual);

            var line1 = new Label("   Phrase");
            contentFolder.Add(line1);

            var phraseVisual = new PhraseVisual(nodeData.PhraseVisualData);
            contentFolder.Add(phraseVisual);

            var line2 = new Label(" ");
            contentFolder.Add(line2);

            var phraseEvents = new PhraseEvents(nodeData.EventVisualData);
            contentFolder.Add(phraseEvents);

            var line3 = new Label(" ");
            contentFolder.Add(line3);

            var phraseContainer = new PhraseElementsTable();
            phraseContainer.Add(new Label("Phrase Assets"));

            for (var i = 0; i < languages.Count; i++)
            {
                var clip = nodeData.PhraseSounds != null && nodeData.PhraseSounds.Count > i
                    ? nodeData.PhraseSounds[i]
                    : null;
                var phrase = nodeData.Phrases != null && nodeData.Phrases.Count > i ? nodeData.Phrases[i] : null;
                phraseContainer.Add(new PhraseElementsRowField(languages[i], clip, phrase));
            }

            contentFolder.Add(phraseContainer);

            var inPort = GraphElements.GeneratePort(this, Direction.Input, Port.Capacity.Multi);
            inPort.portName = AaGraphConstants.InPortName;
            inputContainer.Add(inPort);

            var outPort = GraphElements.GeneratePort(this, Direction.Output, Port.Capacity.Multi);
            outPort.portName = AaGraphConstants.OutPortName;
            outputContainer.Add(outPort);

            RefreshExpandedState();
            RefreshPorts();
            SetPosition(new Rect(Vector2.zero, _defaultNodeSize));
        }

        private string GetTitle(string person, string text)
        {
            return $"{person}\n{text}";
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
    }
}