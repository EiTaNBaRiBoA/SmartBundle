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

        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return www.SendWebRequest();
            if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("Server Request failed " + url + " " + www.error);
            }
            else
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                objectToInstantiate = bundle.LoadAsset(bundle.GetAllAssetNames()[0]) as GameObject;
                if (writeToDisk)
                {
                    string path = Path.GetFullPath(assetPathSave).Replace('/', '\\');
                    Save(objectToInstantiate, path);
                    LoadObject(path);
                }
                else
                {
                    bundle.Unload(false); // If asset is in the game should not be unloaded
                    yield return new WaitForEndOfFrame();

                    if (objectToInstantiate != null)
                    {
                        Instantiate(objectToInstantiate, Vector3.zero, Quaternion.identity);
                    }
                    else
                        Debug.LogError("null Gameobject");
                }
            }
            www.Dispose(); // Unmanaged code
        }
    }


    public bool Save(GameObject obj, string path)
    {

        Debug.Log(path);
        if (!Directory.Exists((path)))
        {
            Directory.CreateDirectory((path));
        }

        path = path + "/Game.dat";
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, obj);

        stream.Close();

        return true;
    }

    private void LoadObject(string path) // For read only access
    {
        GameObject newObj = null;
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);
        try
        {
            newObj = formatter.Deserialize(stream) as GameObject;
            stream.Close();

        }
        catch (Exception e)
        {
            stream.Close();
            Debug.Log(e.Message);
        }
    }
}

// private void saveFile(byte[] data)
// {
//     string path = Path.GetFullPath(assetPathSave).Replace('/', '\\');
//     Debug.Log(path);
//     if (!Directory.Exists((path)))
//     {
//         Directory.CreateDirectory((path));
//     }

//     try
//     {
//         File.WriteAllBytes(assetPathSave, data);
//         Debug.Log("Saved Data to: " + path.Replace("/", "\\"));
//     }
//     catch (Exception e)
//     {
//         Debug.LogWarning("Failed To Save Data to: " + assetPathSave.Replace("/", "\\"));
//         Debug.LogWarning("Error: " + e.Message);
//     }
// }