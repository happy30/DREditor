//Author: Benjamin "Sweden" Jillson : Sweden#6386
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace DREditor.PlayerInfo
{
    
    public class PlayerInfo : MonoBehaviour
    {
        public static PlayerInfo instance = null;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
        }
        private void OnValidate()
        {
            
            // Player Settings
            //UpdatePlayerSettings();

            // Player Stats



            //EditorUtility.SetDirty(this); Saying it doesn't exist when it should
        }
        public static int[] ToIntArray()
        {
            int[] arr = new int[5];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = i;
            }
            return arr;
        }

        #region Saving and Loading
        public Data Save()
        {
            Data d = new Data();
            d.info = Info;
            d.settings = settings;
            d.health = CurrentHealth;
            return d;
        }
        public void Load(Data d)
        {
            Info = d.info;
            settings = d.settings;
            CurrentHealth = d.health > 0 ? d.health : 1;
        }
        [System.Serializable]
        public class Data
        {
            public Information info;
            public PlayerSettings settings;
            public int health;
        }
        #endregion

        [Header("Information")]
        public Information Info = new Information();

        [System.Serializable]
        public class Information
        {
            public bool pauseAccess;
            // Map, TruthBullets, Presents, Reportcard, Regulations
            public bool[] pauseOptions = new bool[]
            {
                false, false, false, false, false
            };

            public List<string> foundBullets = new List<string>();

            public void AddBullet(string name)
            {
                if (foundBullets.Contains(name))
                    return;
                else
                    foundBullets.Add(name);
            }
            public void Clear()
            {
                foundBullets.Clear();
                pauseAccess = false;
                for (int i = 0; i < pauseOptions.Length; i++)
                    pauseOptions[i] = false;
            }
        }
        
        [Header("Player Settings")]
        public PlayerSettings settings = new PlayerSettings();
        [System.Serializable]
        public class PlayerSettings
        {
            [Range(0, 2)] // [Range(-80, 20)] If using unity audio
            public float BGMVolume = 1; // 0
            [Range(0, 2)] // [Range(-80, 20)] If using unity audio
            public float SFXVolume = 1; // 0
            [Range(0, 2)] // [Range(-80, 20)] If using unity audio
            public float VoiceVolume = 1; // 0
            [Range(1, 5)]
            public float TextSpeed = 3;
            [Range(1, 5)]
            public int ReticleSpeed = 3;
            public bool MovementBob = true;
            public bool LookInvert = false;
            public bool InvertX = false;
            public bool DRCameraPan = false;
            public bool Controls = true;
            public bool MapDisplay = true;
            // Language
        }

        [Header("Player Stats")]

        public int MaxHealth = 10;
        public int CurrentHealth = 10;
        public UnityEvent HealthChanged;
        public void TakeDamage(int damage)
        {
            if (CurrentHealth - damage < 0)
                CurrentHealth = 0;
            else
                CurrentHealth -= damage;

            HealthChanged?.Invoke();
        }
        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
            HealthChanged?.Invoke();
        }

        public float MaxStamina = 10;
        public float CurrentStamina = 10;
        public float StaminaRatio = 2f;
        public UnityEvent StaminaChanged;
        public void DrainStamina()
        {
            CurrentStamina -= StaminaRatio * Time.unscaledDeltaTime;
        }
        public void RegenStamina()
        {
            if (CurrentStamina + StaminaRatio * Time.unscaledDeltaTime >= MaxStamina)
            {
                CurrentStamina = MaxStamina;
                return;
            }
                

            CurrentStamina += StaminaRatio * Time.unscaledDeltaTime;
        }
    }
}
