using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DREditor.Characters;
using DREditor.Dialogues;

namespace DREditor.Localization
{
    public class LocaleManager : MonoBehaviour
    {
        public static LocaleManager LM = null;

        public LocaleSettings settings;

        [HideInInspector]
        public Locale activeLocale;

        private Dictionary<string, LocalizedCharacter> localizedCharactersMap;
        private Dictionary<string, LocalizedDialogue> localizedDialoguesMap;
        private Dictionary<string, LocalizedDialogueLine> localizedDialogueLinesMap;
        private Dictionary<string, TranslatableText> translatablesMap;

        void Awake()
        {
            if (LM == null)
            {
                LM = this;
            }
            else if (LM != this)
            {
                Destroy(gameObject);
            }
            if (settings != null)
            {
                activeLocale = settings.defaultLocale;
            }
            DontDestroyOnLoad(gameObject);
        }
        
        public bool LoadLocale(Locale locale)
        {
            bool success = true;
            if(!locale.Equals(activeLocale))
            {
                if(locale.Equals(settings.defaultLocale) && settings.isDefaultEmbeded)
                {
                    // We don't need to load any resource for the default locale because the text is embeded
                    // Clean up the dictionaries
                    localizedCharactersMap = null;
                    localizedDialoguesMap = null;
                    localizedDialogueLinesMap = null;
                    translatablesMap = null;
                } else
                {
                    LocalizedCharacterDatabase cdb = Resources.Load<LocalizedCharacterDatabase>(settings.baseFolder + "/" + locale.langCode + "/Characters/database");
                    if (cdb != null)
                    {
                        localizedCharactersMap = cdb.GetLocalizationMap();
                        activeLocale = locale;
                        success = LoadTranslatableMap(locale);
                    }
                    else
                    {
                        Debug.LogWarning("Localized Character Database not found for locale "+locale.langName+"("+locale.langCode+")");
                        success = false;
                    }
                }
                if(success)
                {
                    activeLocale = locale;
                    // Clean up the unneded resources after loading the locale
                    Resources.UnloadUnusedAssets();
                }
            }
            return success;
        }

        private bool LoadTranslatableMap(Locale locale)
        {
            bool success = true;
            TranslatableTextDatabase tdb = Resources.Load<TranslatableTextDatabase>(settings.baseFolder + "/" + locale.langCode + "/Texts/TranslatablesDatabase");
            if(tdb != null)
            {
                translatablesMap = tdb.GetTranslatablesMap();
            } else
            {
                Debug.LogWarningFormat("Translatables database not found for locale {0}({1})", locale.langName, locale.langCode);
                success = false;
            }
            return success;
        }

        public void LoadDialogues(params Dialogue[] dialogues)
        {
            if (activeLocale.Equals(settings.defaultLocale) && settings.isDefaultEmbeded)
            {
                // If it's embeded there is no need to load nothing
            } else
            {
                // Otherwise we will need to load them
                // First clear up the current maps
                localizedDialoguesMap = null;
                localizedDialogueLinesMap = null;
                if(dialogues != null && dialogues.Length > 0)
                {
                    localizedDialoguesMap = new Dictionary<string, LocalizedDialogue>();
                    localizedDialogueLinesMap = new Dictionary<string, LocalizedDialogueLine>();
                    foreach (Dialogue dia in dialogues)
                    {
                        LocalizedDialogue ld = Resources.Load<LocalizedDialogue>(settings.baseFolder + "/" + activeLocale.langCode + "/Dialogue/" + dia.translationKey);
                        if(ld != null)
                        {
                            localizedDialoguesMap.Add(dia.translationKey, ld);
                            if(ld.Lines != null && ld.Lines.Count > 0)
                            {
                                foreach(LocalizedDialogueLine line in ld.Lines)
                                {
                                    localizedDialogueLinesMap.Add(line.translationKey, line);
                                }
                            }
                        } else
                        {
                            Debug.LogWarningFormat("Localized Dialogue {0} not found for locale {1}({2})", dia.name, activeLocale.langName, activeLocale.langCode);
                        }
                    }
                }
            }
        }

        public LocalizedCharacter GetLocalizedCharacter(Character c)
        {
            LocalizedCharacter found = null;
            if(localizedDialoguesMap == null)
            {
                if(!activeLocale.Equals(settings.defaultLocale) || !settings.isDefaultEmbeded)
                {
                    Debug.LogError("The character database was not loaded for the current locale: " + activeLocale.langName + "(" + activeLocale.langCode + ")");
                }
            } else if (!string.IsNullOrEmpty(c.translationKey) && localizedCharactersMap.ContainsKey(c.translationKey))
            {
                found = localizedCharactersMap[c.translationKey];
            }
            return found;
        }

        public LocalizedDialogue GetLocalizedDialogue(Dialogue dia)
        {
            LocalizedDialogue found = null;
            if(localizedDialoguesMap == null || localizedDialogueLinesMap == null)
            {
                if (!activeLocale.Equals(settings.defaultLocale) || !settings.isDefaultEmbeded)
                {
                    Debug.LogError("The dialogue was not loaded for the current locale: " + activeLocale.langName + "(" + activeLocale.langCode + ")");
                }
            } else if (!string.IsNullOrEmpty(dia.translationKey) && localizedDialoguesMap.ContainsKey(dia.translationKey))
            {
                found = localizedDialoguesMap[dia.translationKey];
            }
            return found;
        }

        public LocalizedDialogueLine GetLocalizedDialogueLine(Line line)
        {
            LocalizedDialogueLine found = null;
            if(localizedDialoguesMap == null || localizedDialogueLinesMap == null)
            {
                if(!activeLocale.Equals(settings.defaultLocale) || !settings.isDefaultEmbeded)
                {
                    Debug.LogError("The dialogue was not loaded for the current locale: " + activeLocale.langName + "(" + activeLocale.langCode + ")");
                }
            }
            else if (!string.IsNullOrEmpty(line.translationKey) && localizedDialogueLinesMap.ContainsKey(line.translationKey))
            {
                found = localizedDialogueLinesMap[line.translationKey];
            }
            return found;
        }

        public void CleanUpDialogues()
        {
            localizedDialogueLinesMap = null;
            localizedDialoguesMap = null;
            Resources.UnloadUnusedAssets();
        }

        public TranslatableText GetLocalizedText(TranslatableText text)
        {
            TranslatableText result = text; // Default to the original text
            if(translatablesMap == null)
            {
                if (!activeLocale.Equals(settings.defaultLocale) || !settings.isDefaultEmbeded)
                {
                    Debug.LogWarningFormat("The translatables map was not loaded for the current locale {0}({1})", activeLocale.langName, activeLocale.langCode);
                }
            } else if(!string.IsNullOrEmpty(text.translationKey) && translatablesMap.ContainsKey(text.translationKey))
            {
                result = translatablesMap[text.translationKey];
            }
            return result;
        }
    }
}

