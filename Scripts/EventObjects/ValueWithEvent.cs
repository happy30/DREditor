//
// EventObjects - A scriptable-object based messaging system for Unity
//
// Copyright (c) 2019 Bart Heijltjes (Wispfire)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using UnityEngine;
using UnityEngine.Events;

namespace EventObjects
{
    public abstract class BaseValueWithEvent : ScriptableObject
    {
        public abstract void Invoke();
        public abstract bool IsInitialized { get; protected set; }
        public abstract void Reset();
        public abstract void Init();

    }
    
    public abstract class ValueWithEvent<T,TY> : BaseValueWithEvent where TY : UnityEvent<T>, new()
    {
        public TY OnChange = new TY();

        public T InitialValue;

        public override bool IsInitialized { get; protected set; }

        [SerializeField] private T _value;
        public T Value
        {
            get
            {
                if (!IsInitialized) Init();
                return _value;
            }
            set => SetValue(value);
        }

        /// <summary>
        /// Set a new value and invoke the change event if it is different.
        /// </summary>
        public virtual void SetValue(T x)
        {
            if (!IsInitialized) IsInitialized = true;

            if (x != null)
            {
                if (!x.Equals(_value))
                {
                    _value = x;
                    OnChange.Invoke(x);
                }
            }
            else
            {
                _value = default(T); 
            }
                
        }

        /// <summary>
        /// Calls OnChange with the current value.
        /// </summary>
        public override void Invoke()
        {
            OnChange.Invoke(Value);
        }

        /// <summary>
        /// Sets the initial value and marks the object initialized.
        /// </summary>
        public override void Init()
        {
            _value = InitialValue;
            IsInitialized = true;
        }

        /// <summary>
        /// Get current value of the object and add a listener.
        /// </summary>
        /// <param name="action">Action that should happen on value change</param>
        public virtual T GetValueAndAddListener(UnityAction<T> action)
        {
            if (!IsInitialized) Init();
            OnChange.AddListener(action);
            return Value;
        }

        public void RemoveListener(UnityAction<T> action)
        {
            OnChange.RemoveListener(action);
        }

        /// <summary>
        /// Reset the object's fields.
        /// </summary>
        public override void Reset()
        {
            _value = default(T);
            IsInitialized = false;
        }

        // In the editor, reset IsInitialized value when changing playmode.
#if UNITY_EDITOR
        void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += PlayModeStateChange;
        }

        void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= PlayModeStateChange;

        }

        private void PlayModeStateChange(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode || state == UnityEditor.PlayModeStateChange.ExitingEditMode) Reset();
        }
#endif
        
    }
    
    /// <summary>
    /// This caches a ValueWithEvent to optimize it for fast read access. 
    /// </summary>
    /// <typeparam name="T">ValueWithEvent Type</typeparam>
    /// <typeparam name="TX">Value Type</typeparam>
    /// <typeparam name="TY">UnityEvent derived Type used by ValueWithEvent Type</typeparam>
    public class CachedVariable<T,TX,TY> where T : ValueWithEvent<TX, TY> where TY : UnityEvent<TX>, new() // TODO: Add option to use local value instead of BoolWithEvent
    {        
        public T SyncWith;

        private TX _currentValue;
        private bool _isSetup;

        public TX Value
        {
            get => _currentValue;
            set => SyncWith.Value = value;
        }

        /// <summary>
        /// Registers the caching variable with its backing value and sets up listeners.
        /// Call this before anything else and make sure to call Unregister() as well. 
        /// </summary>
        public void Register()
        {
            if (_isSetup) return;
            if (SyncWith == null)
            {
                Debug.LogWarning("SyncWith field is not filled. This is not supported.");
            }
            
            _currentValue = SyncWith.GetValueAndAddListener((x) => _currentValue = x);
            _isSetup = true;
        }
        
        /// <summary>
        /// Removes the event listener from the backing value and deinitializes the caching variable.
        /// </summary>
        public void Unregister()
        {
            SyncWith.OnChange.RemoveListener(sync);
            _isSetup = false;
        }

        void sync(TX value)
        {
            _currentValue = value;
        }
        
    }
    
    
    [Serializable] public class BoolEvent : UnityEvent<bool>{}
    [Serializable] public class IntEvent : UnityEvent<int>{}
    [Serializable] public class FloatEvent : UnityEvent<float>{}
    [Serializable] public class SpriteEvent : UnityEvent<Sprite>{}
    [Serializable] public class StringArrayEvent : UnityEvent<string[]>{}
    [Serializable] public class GameObjectEvent : UnityEvent<GameObject>{}
    
    [Serializable] public class TransformEvent : UnityEvent<PosRot>{}
    

    
}
