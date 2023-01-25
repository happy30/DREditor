﻿using System.Collections;
using System.Collections.Generic;
using DREditor.EventObjects;
using UnityEngine;

namespace DREditor.Camera
{

    public class CameraShake : MonoBehaviour
    {
        public bool Enabled;
        public FloatWithEvent Omega;

        public Vector3 GetRandomOffset()
        {
            var x = Random.Range(-1f, 1f) * Omega.Value;
            var y = Random.Range(-1f, 1f) * Omega.Value;
            return new Vector3(x, y, 0);

        }
    }
}