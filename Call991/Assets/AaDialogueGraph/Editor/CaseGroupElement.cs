using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AaDialogueGraph.Editor
{
    public class CaseGroupElement : VisualElement
    {
        public CaseGroupElement(Foldout foldout, List<CaseData> wordData, List<EndData> endData,
            List<CountData> countData)
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
                }, AaKeys.ChoiceKeys));
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
                }, AaKeys.ChoiceKeys));
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
                }, AaKeys.EndKeys));
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
                }, AaKeys.EndKeys));
                UpdateCasesCount(foldout);
            });
            noEndCase.text = AaGraphConstants.NoEnd;
            noEndCase.AddToClassList("aa-ChoiceAsset_content-container-pink");
            buttonsContainer.Add(noEndCase);

            var countCase = new Button(() =>
            {
                contentContainer.Add(new CountCase(element =>
                {
                    RemoveElement(element, contentContainer);
                    UpdateCasesCount(foldout);
                }, AaKeys.CountKeys));
                UpdateCasesCount(foldout);
            });
            countCase.text = AaGraphConstants.PlusCount;
            countCase.AddToClassList("aa-ChoiceAsset_content-container-orange");
            buttonsContainer.Add(countCase);

            CreateWordCases(foldout, wordData);
            CreateEndCases(foldout, endData);
            //CreateCountCases(foldout, countData);

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
            var cntCounts = foldout.Query<CountCase>().ToList().Count;
            cnt = cntWords + cntEnds + cntCounts;

            foldout.text = $"Cases {cnt}";
        }

        private void CreateWordCases(VisualElement foldout, List<CaseData> data)
        {
            if (data == null || data.Count < 1) return;

            foreach (var caseData in data)
            {
                if (caseData?.OrKeys == null || caseData.OrKeys.Count < 1) continue;
                ChoiceCase choiceCase;

                switch (caseData.CaseType)
                {
                    case CaseType.AndWord:
                        choiceCase = new AndChoiceCase(AaGraphConstants.And, element =>
                            RemoveElement(element, foldout), AaKeys.ChoiceKeys, caseData.OrKeys);
                        break;
                    case CaseType.NoWord:
                        choiceCase = new NoChoiceCase(AaGraphConstants.No, element =>
                            RemoveElement(element, foldout), AaKeys.ChoiceKeys, caseData.OrKeys);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                foldout.Add(choiceCase);
            }
        }
        private void CreateEndCases(VisualElement foldout, List<EndData> data)
        {
            if (data == null || data.Count < 1) return;

            foreach (var caseData in data)
            {
                if (caseData?.OrKeys == null || caseData.OrKeys.Count < 1) continue;
                EndCase endCase;

                switch (caseData.EndType)
                {
                    case EndType.AndEnd:
                        endCase = new AndEndCase(AaGraphConstants.And, element =>
                            RemoveElement(element, foldout), AaKeys.EndKeys, caseData.OrKeys);
                        break;
                    case EndType.NoEnd:
                        endCase = new NoEndCase(AaGraphConstants.No, element =>
                            RemoveElement(element, foldout), AaKeys.EndKeys, caseData.OrKeys);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                foldout.Add(endCase);
            }
        }
    }
}