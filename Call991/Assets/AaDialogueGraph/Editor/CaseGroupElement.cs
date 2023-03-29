using System;
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

            var andWordCase = new Button(() =>
            {
                contentContainer.Add(new AndChoiceCase(AaGraphConstants.And, element =>
                {
                    RemoveElement(element, contentContainer);
                    UpdateCasesCount(foldout);
                }, AaChoices.ChoiceKeys));
                UpdateCasesCount(foldout);
            });
            andWordCase.text = AaGraphConstants.AndWord;
            andWordCase.AddToClassList("aa-ChoiceAsset_content-container-green");
            buttonsContainer.Add(andWordCase);

            var noWordCase = new Button(() =>
            {
                contentContainer.Add(new NoChoiceCase(AaGraphConstants.No, element =>
                {
                    RemoveElement(element, contentContainer);
                    UpdateCasesCount(foldout);
                }, AaChoices.ChoiceKeys));
                UpdateCasesCount(foldout);
            });
            noWordCase.text = AaGraphConstants.NoWord;
            noWordCase.AddToClassList("aa-ChoiceAsset_content-container-red");
            buttonsContainer.Add(noWordCase);
            
            var addEndCase = new Button(() =>
            {
                contentContainer.Add(new AndEndCase(AaGraphConstants.And, element =>
                {
                    RemoveElement(element, contentContainer);
                    UpdateCasesCount(foldout);
                }, AaEnds.EndKeys));
                UpdateCasesCount(foldout);
            });
            addEndCase.text = AaGraphConstants.AndEnd;
            addEndCase.AddToClassList("aa-ChoiceAsset_content-container-blue");
            buttonsContainer.Add(addEndCase);
            
            var noEndCase = new Button(() =>
            {
                contentContainer.Add(new NoEndCase(AaGraphConstants.No, element =>
                {
                    RemoveElement(element, contentContainer);
                    UpdateCasesCount(foldout);
                }, AaEnds.EndKeys));
                UpdateCasesCount(foldout);
            });
            noEndCase.text = AaGraphConstants.NoEnd;
            noEndCase.AddToClassList("aa-ChoiceAsset_content-container-pink");
            buttonsContainer.Add(noEndCase);
            //
            //
            // todo add count case
            //
            //
            CreateCases(foldout, data);

            UpdateCasesCount(foldout);
        }

        private void RemoveElement(VisualElement element, VisualElement container)
        {
            container.Remove(element);
        }
        private void UpdateCasesCount(Foldout foldout)
        {
            var cnt = 0;
            var cntWords = foldout.Query<ChoiceCase>().ToList().Count;
            var cntEnds = foldout.Query<EndCase>().ToList().Count;
            //var cntCounts = foldout.Query<EndCase>().ToList().Count;
            cnt = cntWords + cntEnds;
            
            foldout.text = $"Cases {cnt}";
        }
        
        private void CreateCases(Foldout foldout, List<CaseData> data)
        {
            if (data == null || data.Count <1 ) return;
                
            foreach (var caseData in data)
            {
                if (caseData?.Cases == null || caseData.Cases.Count < 1) continue;

                if (caseData.CaseType is CaseType.AndWord or CaseType.NoWord)
                {
                    ChoiceCase choiceCase;

                    switch (caseData.CaseType)
                    {
                        case CaseType.AndWord:
                            choiceCase = new AndChoiceCase(AaGraphConstants.And, element =>
                                RemoveElement(element, foldout), AaChoices.ChoiceKeys, caseData.Cases[0]);
                            break;
                        case CaseType.NoWord:
                            choiceCase = new NoChoiceCase(AaGraphConstants.No, element =>
                                RemoveElement(element, foldout), AaChoices.ChoiceKeys, caseData.Cases[0]);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    foldout.Add(choiceCase);
                }

                if (caseData.CaseType is CaseType.AndEnd or CaseType.NoEnd)
                {
                    EndCase endCase;

                    switch (caseData.CaseType)
                    {
                        case CaseType.AndEnd:
                            endCase = new AndEndCase(AaGraphConstants.And, element =>
                                RemoveElement(element, foldout), AaEnds.EndKeys, caseData.Cases[0]);
                            break;
                        case CaseType.NoEnd:
                            endCase = new NoEndCase(AaGraphConstants.No, element =>
                                RemoveElement(element, foldout), AaEnds.EndKeys, caseData.Cases[0]);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    for (var i = 1; i < caseData.Cases.Count; i++)
                    {
                        endCase.AddCaseField(caseData.Cases[i]);
                    }

                    foldout.Add(endCase);
                }

                if (caseData.CaseType is CaseType.Count)
                {
                    // todo need to add count case generation
                    throw new NotImplementedException();
                }
            }
        }
    }
}