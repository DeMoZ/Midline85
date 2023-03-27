using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class CaseGroupElement : VisualElement
    {
        public CaseGroupElement(Foldout foldout, List<CaseData> data)
        {
            var buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Row;
            foldout.Add(buttonsContainer);

            var addAndCase = new Button(() =>
            {
                contentContainer.Add(new AndChoiceCase("and", element =>
                {
                    RemoveElement(element, contentContainer);
                    UpdateCasesCount(foldout);
                }, AaChoices.ChoiceKeys));
                UpdateCasesCount(foldout);
            });
            addAndCase.text = AaGraphConstants.AndWord;
            buttonsContainer.Add(addAndCase);

            var addNoCase = new Button(() =>
            {
                contentContainer.Add(new NoChoiceCase("no", element =>
                {
                    RemoveElement(element, contentContainer);
                    UpdateCasesCount(foldout);
                }, AaChoices.ChoiceKeys));
                UpdateCasesCount(foldout);
            });
            addNoCase.text = AaGraphConstants.NoWord;
            buttonsContainer.Add(addNoCase);
            
            CreateCases(foldout, data);

            UpdateCasesCount(foldout);
        }

        private void RemoveElement(VisualElement element, VisualElement container)
        {
            container.Remove(element);
        }
        private void UpdateCasesCount(Foldout foldout)
        {
            var cnt = foldout.Query<ChoiceCase>().ToList().Count;
            foldout.text = $"Cases {cnt}";
        }
        
        private void CreateCases(Foldout foldout, List<CaseData> data)
        {
            if (data == null || data.Count <1 ) return;
                
            foreach (var caseData in data)
            {
                if (caseData?.Cases == null || caseData.Cases.Count < 1) continue;

                ChoiceCase aCase;
                if (caseData.And)
                {
                    aCase = new AndChoiceCase("and", element =>
                        RemoveElement(element, foldout), AaChoices.ChoiceKeys, caseData.Cases[0]);
                }
                else
                {
                    aCase = new NoChoiceCase("no", element =>
                        RemoveElement(element, foldout), AaChoices.ChoiceKeys, caseData.Cases[0]);
                }

                for (var i = 1; i < caseData.Cases.Count; i++)
                {
                    aCase.AddCaseField(caseData.Cases[i]);
                }

                foldout.Add(aCase);
            }
        }
    }
}