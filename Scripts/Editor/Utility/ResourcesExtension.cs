using System.IO;
using UnityEngine;
namespace DREditor.Utility.Editor
{
    public static class ResourcesExtension
    {
        public static string ResourcesPath = Application.dataPath + "/Resources";

        public static T Load<T>(string resourceName) where T : UnityEngine.Object
        {
            string[] directories = Directory.GetDirectories(ResourcesPath, "*", SearchOption.AllDirectories);
            foreach (var item in directories)
            {
                string itemPath = item.Substring(ResourcesPath.Length + 1);
                var result = Resources.Load<T>(itemPath + "\\" + resourceName);
                if (result != null) return result;
            }
            return null;
        }
    }
}