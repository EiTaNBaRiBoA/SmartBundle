using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadAssetBundle : MonoBehaviour
{
    #region Const Var
    private const int ZIP_LEAD_BYTES = 0x04034b50;
    private const ushort GZIP_LEAD_BYTES = 0x8b1f;
    #endregion


    #region Test Urls

    public string url = "https://drive.google.com/uc?id=1IWoxbMErjpE_oo8MWMeFCp0dEzcPcGxX&export=download";
    public string urlzip = "https://drive.google.com/uc?id=15hBVXzhHQO0HoxKh6vVRBlZ8KiHpfkKr&export=download";
    public bool writeToDisk = false;
    public bool shouldDownloadZip = false;

    #endregion

    #region Properties
    private string assetPathSave;
    private GameObject objectToInstantiate = null;

    #endregion

    #region Static Methods
    /// <param name="data">reading file bytes</param>
    /// <returns>if is compressed type PK</returns>
    public static bool IsPkZipCompressedData(byte[] data)
    {
        Debug.Assert(data != null && data.Length >= 4);
        // if the first 4 bytes of the array are the ZIP signature then it is compressed data
        return (BitConverter.ToInt32(data, 0) == ZIP_LEAD_BYTES);
    }

    /// <param name="data">reading file bytes</param>
    /// <returns>if is compressed type Gzip</returns>
    public static bool IsGZipCompressedData(byte[] data)
    {
        Debug.Assert(data != null && data.Length >= 2);
        // if the first 2 bytes of the array are theG ZIP signature then it is compressed data;
        return (BitConverter.ToUInt16(data, 0) == GZIP_LEAD_BYTES);
    }
    #endregion


    void Start()
    {
        assetPathSave = Application.dataPath + "/../DownloadedAssets";
        StartCoroutine(WebRequestQuery());
    }
    private IEnumerator WebRequestQuery()
    {
        UnityWebRequest www;
        if (writeToDisk)
            www = UnityWebRequest.Get(shouldDownloadZip ? urlzip : url);
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
                string folderPath = Path.GetFullPath(assetPathSave);
                string assetFileName = Path.Combine(assetPathSave, "cubebundle"); //The name of the asset to instantiate the gameobject
                Save(www.downloadHandler.data, folderPath, assetPathSave, assetFileName);
            }
            else
            {
                StartCoroutine(AssetNotStoredDisk(DownloadHandlerAssetBundle.GetContent(www)));
            }
            www.Dispose();
        }
    }


    public async void Save(byte[] data, string folder, string file, string assetFileName)
    {
        if (!Directory.Exists((folder)))
        {
            Directory.CreateDirectory((folder));
        }
        bool shouldUnZip = false;
        if (IsPkZipCompressedData(data) || IsGZipCompressedData(data))
        {
            shouldUnZip = true;
            file = Path.Combine(assetPathSave, "Asset.zip");
        }
        else
            file = Path.Combine(assetPathSave, "AssetBundle");


        try
        {

            if (!File.Exists(file))
                File.WriteAllBytes(file, data); // will access denied if folder path
            if (shouldUnZip)
            {
                Debug.Log("Unzip");
                await Task.Run(() => ZipFile.ExtractToDirectory(file, folder, true));
                File.Delete(file);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed To Save Data to: " + folder);
            Debug.LogWarning("Error: " + e.Message);
        }
        finally
        {
            StartCoroutine(LoadObject(assetFileName));
        }
    }
    public GameObject testobject;
    IEnumerator LoadObject(string filePath)
    {
        AssetBundleCreateRequest bundle = AssetBundle.LoadFromFileAsync(filePath);
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

    private IEnumerator AssetNotStoredDisk(AssetBundle bundle)
    {
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

}