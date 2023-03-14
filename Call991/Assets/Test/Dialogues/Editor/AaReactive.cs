using System;

namespace Test.Dialogues
{
    public class AaReactive<T> : IDisposable
    {
        private Action<T> _action;
        private T _value;

        public T Value
        {
            //get { return _value; }

            set
            {
                _value = value;
                _action?.Invoke(_value);
            }
        }

        public void Subscribe(Action<T> callback)
        {
            _action += callback;
        }
        
        public void Dispose()
        {
        }
    }
}