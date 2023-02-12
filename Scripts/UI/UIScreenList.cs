//UI Screen List Script by SeleniumSoul for DREditor
//April 2021

using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.AddressableAssets;
//using UnityEngine.ResourceManagement.AsyncOperations;

public class UIScreenList : MonoBehaviour
{
    //public List<AssetReference> UIScreens;

    public List<GameObject> LoadedScreens;

    public void LoadUIScreen(int _selection)
    {
        //AsyncOperationHandle _screenLoad = Addressables.LoadAssetAsync<GameObject>(UIScreens[_selection]);

        //if (_screenLoad.IsDone)
        //{
        //    LoadedScreens.Add((GameObject)_screenLoad.Result);
        //}
    }
}
