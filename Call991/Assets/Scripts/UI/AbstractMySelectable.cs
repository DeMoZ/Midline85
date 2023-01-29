using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AbstractMySelectable : Selectable
    {
        public event Action OnClick;
        public event Action<GameObject> OnAnySelect;

        protected static AbstractMySelectable currentSelection;

        protected virtual void Update()
        {
            if (currentSelection == this && Input.GetKey(KeyCode.Return))
                OnClick?.Invoke();
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            switch (state)
            {
                case SelectionState.Pressed:
                    OnClick?.Invoke();
                    break;

                case SelectionState.Selected:
                    if (AlreadySelected()) 
                        return;

                    OnAnySelect?.Invoke(gameObject);
                    break;
            }
        }

        private bool AlreadySelected() => !gameObject.NotSelected();
    }
}