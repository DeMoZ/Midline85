using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Test.Dialogues
{
    public class PhraseNode : AaNode
    {
        private string _personTxt;
        private string _phraseSketchTxt;

        public string PhraseSketchText => _phraseSketchTxt;
        
        public PhraseNode(PhraseNodeData nodeData, List<string> languages, string guid = null)
        {
            Guid = guid ?? System.Guid.NewGuid().ToString();

            titleContainer.Add(new NodeTitleErrorField());
            
            var contentFolder = new Foldout();
            contentFolder.value = false;
            contentContainer.Add(contentFolder);
            
            var titleTextField = new PhraseSketchField(nodeData.PhraseSketchText, val =>
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

            var phraseEvents = new PhraseEvents(nodeData.EventVisualData, CheckNodeContent);
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
                phraseContainer.Add(new PhraseElementsRowField(languages[i], clip, phrase, CheckNodeContent));
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
                errorFields.Insert(0,"<color=red>");
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