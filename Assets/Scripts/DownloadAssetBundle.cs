using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadAssetBundle : MonoBehaviour
{
    public string url = "https://drive.google.com/uc?id=1IWoxbMErjpE_oo8MWMeFCp0dEzcPcGxX&export=download";
    public bool writeToDisk = false;
    string assetPathSave;
    private GameObject objectToInstantiate = null;
    // Start is called before the first frame update
    void Start()
    {
        assetPathSave = Application.dataPath + "/../DownloadedAssets";
        StartCoroutine(DownloadSingleAssetBundle());


    }
    private IEnumerator DownloadSingleAssetBundle()
    {
        UnityWebRequest www;
        if (writeToDisk)
            www = UnityWebRequest.Get(url);
        else
            www = UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return www.SendWebRequest();
        if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogWarning("Server Request failed " + url + " " + www.error);
        }
        else
        {

            if (writeToDisk)
            {
                string path = Path.GetFullPath(assetPathSave).Replace('/', '\\');
                if (!Directory.Exists((path)))
                {
                    Directory.CreateDirectory((path));
                }
                assetPathSave = Path.Combine(assetPathSave, "r.unity3d");
                //Save(www.downloadHandler.data, assetPathSave);
                StartCoroutine(LoadObject(assetPathSave));
            }
            else
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                objectToInstantiate = bundle.LoadAsset(bundle.GetAllAssetNames()[0]) as GameObject;
                bundle.Unload(false); // If asset is in the game should not be unloaded
                yield return new WaitForEndOfFrame();

                if (objectToInstantiate != null)
                {
                    Instantiate(objectToInstantiate, Vector3.zero, Quaternion.identity);
                }
                else
                    Debug.LogError("null Gameobject");
            }
            www.Dispose(); // Unmanaged code
        }
    }


    public void Save(byte[] obj, string path)
    {

        Debug.Log(path);

        try
        {
            File.WriteAllBytes(path, obj);
            Debug.Log("Saved Data to: " + path.Replace("/", "\\"));
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed To Save Data to: " + path.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
        }
    }
    public GameObject testobject;
    IEnumerator LoadObject(string path)
    {
        AssetBundleCreateRequest bundle = AssetBundle.LoadFromFileAsync(path);
        yield return bundle;

        AssetBundle myLoadedAssetBundle = bundle.assetBundle;
        if (myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            yield break;
        }

        AssetBundleRequest request = myLoadedAssetBundle.LoadAssetAsync<GameObject>(myLoadedAssetBundle.GetAllAssetNames()[0]);
        yield return request;

        testobject = request.asset as GameObject;
        var obj = myLoadedAssetBundle.LoadAsset(myLoadedAssetBundle.GetAllAssetNames()[0]) as GameObject;
        Instantiate(obj, Vector3.zero, Quaternion.identity);

        myLoadedAssetBundle.Unload(false);
    }
}