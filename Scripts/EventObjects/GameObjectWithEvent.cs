﻿using System;
using UnityEngine;
using UnityEngine.Events;

namespace DREditor.EventObjects
{

    [Serializable]
    [CreateAssetMenu(menuName = "DREditor/EventObjects/GameObject", fileName = "New GameObject")]
    public class GameObjectWithEvent : ValueWithEvent<GameObject, GameObjectEvent>
    {
        public bool Resolve()
        {
            return true;
        }
    }
}