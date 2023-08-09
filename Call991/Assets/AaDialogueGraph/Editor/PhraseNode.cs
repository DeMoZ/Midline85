using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class PhraseNode : AaNode
    {
        private string _personTxt;
        private string _phraseSketchTxt;

        public string PhraseSketchText => _phraseSketchTxt;

        public void Set(PhraseNodeData data, List<string> languages, 
            List<string> voices, List<string> musics, List<string> rtpcs, List<string> sounds, string guid)
        {
            Guid = guid;

            titleContainer.Add(new NodeTitleErrorField());

            var contentFolder = new Foldout();
            contentFolder.value = false;
            extensionContainer.Add(contentFolder);
            contentFolder.AddToClassList("aa-PhraseNode_extension-container");

            var titleTextField = new PhraseSketchField();
            titleTextField.Set(data.PhraseSketchText, val =>
            {
                _phraseSketchTxt = val;
                title = GetTitle(_personTxt, _phraseSketchTxt);
            });
            contentFolder.Add(titleTextField);

            var personVisual = new PersonVisual();
            personVisual.Set(data.PersonVisualData, val =>
            {
                _personTxt = AaKeys.PersonsKeys.GetColorKey(val);

                title = GetTitle(_personTxt, _phraseSketchTxt);
            });
            contentFolder.Add(personVisual);

            var phraseVisual = new PhraseVisual();
            phraseVisual.Set(data.PhraseVisualData);
            contentFolder.Add(phraseVisual);

            var phraseEvents = new AaNodeEvents();
            phraseEvents.Set(data.EventVisualData, CheckNodeContent, sounds, musics, rtpcs);
            contentFolder.Add(phraseEvents);

            var phraseContainer = new ElementsTable();

            var soundText = new Label("Phrase Sound");
            soundText.AddToClassList("aa-BlackText");

            voices = voices == null || voices.Count < 1 ? new List<string> { AaGraphConstants.None } : voices;
            var sound = !string.IsNullOrEmpty(data.PhraseSound)
                ? data.PhraseSound
                : AaGraphConstants.None;

            var soundPopup = new SoundPopupField(voices, sound)
            {
                name = AaGraphConstants.VoicePopupField
            };

            var soundLine = new LineGroup(new VisualElement[] { soundText, soundPopup });

            phraseContainer.Add(soundLine);

            var phraseAssetsLabel = new Label("Phrase Assets");
            phraseAssetsLabel.AddToClassList("aa-BlackText");
            phraseContainer.Add(phraseAssetsLabel);

            for (var i = 0; i < languages.Count; i++)
            {
                var phrase = data.Phrases != null && data.Phrases.Count > i
                    ? NodeUtils.GetObjectByPath<Phrase>(data.Phrases[i])
                    : null;

                var phraseRowField = new PhraseElementsRowField();
                phraseRowField.Set(languages[i], phrase, CheckNodeContent);
                phraseContainer.Add(phraseRowField);
            }

            phraseContainer.contentContainer.AddToClassList("aa-PhraseAsset_content-container");

            contentFolder.Add(phraseContainer);

            CreateInPort();
            CreateOutPort();

            RefreshExpandedState();
            RefreshPorts();
            CheckNodeContent();
        }

        public void CheckNodeContent()
        {
            var phrases = contentContainer.Query<PhraseAssetField>().ToList();
            var events = contentContainer.Query<EventAssetField>().ToList();
            
            var sounds = contentContainer.Query<SoundPopupField>().ToList();
            var phraseSound = sounds.FirstOrDefault(s => s.name == AaGraphConstants.VoicePopupField);

            var errorFields = new StringBuilder();

            if (phrases.Any(p => p.GetPhrase() == null))
                errorFields.Append("p.");

            if (phraseSound == null || phraseSound.Value == AaGraphConstants.None)
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

        public string GetPhraseSound() =>
            contentContainer.Query<SoundPopupField>().ToList()
                .First(field => field.name == AaGraphConstants.VoicePopupField).Value;

        public List<Phrase> GetPhrases() =>
            contentContainer.Query<PhraseAssetField>().ToList().Select(field => field.GetPhrase()).ToList();

        public PersonVisual GetPersonVisual() =>
            contentContainer.Q<PersonVisual>();
        
        public ImagePersonVisual GetImagePersonVisual() =>
            contentContainer.Q<ImagePersonVisual>();

        public PhraseVisual GetPhraseVisual() =>
            contentContainer.Q<PhraseVisual>();

        private string GetTitle(string person, string text)
        {
            return $"{person}\n{text}";
        }
    }
}