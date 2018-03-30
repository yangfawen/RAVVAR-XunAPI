using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Ionic.Zip;
using System;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class AssetBuildToolingUI : EditorWindow
{

    static AssetBuildToolingUI ui = null;
    [MenuItem("RAVVAR/Select Prefab")]
    public static void BuildBySelect()
    {
        if (ui != null)
        {
            ui.Close();
        }
        ui = EditorWindow.CreateInstance<AssetBuildToolingUI>();
        ui.Show();
    }
    static void CloseUI()
    {
        if (ui != null)
        {
            ui.Close();
        }
    }

    static Vector2 pos = Vector2.zero;
    static string outpath = string.Empty;
    static string againOutpath = string.Empty;
    static bool isCanleBulid = false;
    static bool isRightSelectObjs = false;
    static string darSeekAssetbundleDefaultFilePath = string.Empty;
    static bool isContinueMovFile = false;
    static bool isErrorEx = false;
    static List<DirectoryInfo> movFilePath = new List<DirectoryInfo>();
    void OnGUI()
    {
        GUILayout.Space(20);
        titleContent = new GUIContent("AssetBuilding");
        GUILayout.Space(20);
        var objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        GUILayout.Label("Selected:" + objs.Length);
        GUILayout.Space(10);
        pos = EditorGUILayout.BeginScrollView(pos);
        for (int i = 0; i < objs.Length; i++)
        {
            if (i == 0)
                EditorGUILayout.LabelField("-------------------------------------------");
            EditorGUILayout.LabelField(objs[i].name);
            EditorGUILayout.LabelField("    " + AssetDatabase.GetAssetPath(objs[i]));
            EditorGUILayout.LabelField("-------------------------------------------");
        }
        EditorGUILayout.EndScrollView();
        GUILayout.Space(20);
        if (GUILayout.Button("Build"))
        {
            Caching.CleanCache();
            isCanleBulid = false; isContinueMovFile = true;
            SetPrefabAssetBundleName(objs);
            JudgeSelectPrefab(objs);
            CheckDarSeekAssetbundleDir();
            if (objs.Length == 1 && isRightSelectObjs)
            {
                UnityEngine.Object obj = Selection.activeObject as UnityEngine.Object;
                outpath = EditorUtility.SaveFilePanel(" ", darSeekAssetbundleDefaultFilePath, obj.name, "zip");
                SelectDataEvent(outpath, obj);
            }
            if (objs.Length > 1 && isRightSelectObjs)
            {
                outpath = EditorUtility.OpenFolderPanel(" ", darSeekAssetbundleDefaultFilePath, "");
                SelectDatasEvent(outpath);
            }

            if (isCanleBulid)
            {
                movFilePath = CreatePrefabDirectory(objs);
                PackageAssetbundleEvent(objs, BuildTarget.Android, "android");
                PackageAssetbundleEvent(objs, BuildTarget.iOS, "ios");
                if (isContinueMovFile)
                {
                    DeletFile(outpath + "/assets"); 
                    if (!isErrorEx)
                    {
                        SetSingleZipName(outpath, objs);
                        CompressEvent(outpath, objs);
                        DeletAssetbundleDirectory(outpath, objs);
                    }
                }
            }
            AssetDatabase.Refresh();
            CloseUI();
        }
    }

    /// <summary>
    ///Status events at radio time
    /// </summary>
    static void SelectDataEvent(string outpath, UnityEngine.Object obj)
    {
        bool isSelectDir = false;
        while (!isSelectDir && outpath != string.Empty)
        {
            CheckDarSeekAssetbundleDir();
            string clearFilePath = Path.GetDirectoryName(outpath);

            if (Directory.GetDirectories(clearFilePath).Length == 0)
            {
                isSelectDir = true; isCanleBulid = true;
            }
            if (Directory.GetDirectories(clearFilePath).Length > 0)
            {
                isSelectDir = false;
                bool isDisplayDialog = EditorUtility.DisplayDialog("Tips", "Select paths that cannot contain folders", "Reselect", "Close");
                if (isDisplayDialog)
                {
                    outpath = EditorUtility.SaveFilePanel(" ", "", obj.name, "zip"); CloseUI(); isCanleBulid = true;
                }
                if (!isDisplayDialog)
                {
                    CloseUI(); isSelectDir = true; isCanleBulid = false;
                }
            }
        }
        againOutpath = outpath;
        AssetDatabase.Refresh();
    }

    /// <summary>
    ///The multi state event
    /// </summary>
    static void SelectDatasEvent(string clearFilePath)
    {
        bool isDisplayDialog = false;
        while (!isDisplayDialog && clearFilePath != string.Empty)
        {
            CheckDarSeekAssetbundleDir();
            if (Directory.GetDirectories(clearFilePath).Length == 0)
            {
                isDisplayDialog = true; isCanleBulid = true;
            }
            if (Directory.GetDirectories(clearFilePath).Length > 0 || (clearFilePath.Substring(clearFilePath.LastIndexOf("/")) == "/Assets"))
            {
                bool isShowDisplayDialog = EditorUtility.DisplayDialog("Tips", "Select paths that cannot contain folders", "Reselect", "Close");
                if (isShowDisplayDialog)
                {
                    clearFilePath = EditorUtility.OpenFolderPanel("", Application.dataPath, "");
                }
                if (!isShowDisplayDialog)
                {
                    clearFilePath = darSeekAssetbundleDefaultFilePath;
                    isDisplayDialog = true; isCanleBulid = false;
                }
            }
        }
        againOutpath = clearFilePath;
        AssetDatabase.Refresh(); CloseUI();
    }

    /// <summary>
    /// Determine whether there is an Assetbundle directory
    /// </summary>
    static void CheckDarSeekAssetbundleDir()
    {
        darSeekAssetbundleDefaultFilePath = Path.GetDirectoryName(Application.dataPath) + "/Assetbundle";
        if (!Directory.Exists(darSeekAssetbundleDefaultFilePath))
        {
            Directory.CreateDirectory(darSeekAssetbundleDefaultFilePath);
        }
    }

    /// <summary>
    ///Compressed events
    /// </summary>
    static void CompressEvent(string zipPath, UnityEngine.Object[] objs)
    {
        zipPath = againOutpath;
        if (zipPath != string.Empty)
        {
            string[] zipDirectories = Directory.GetDirectories(zipPath);
            if (zipDirectories.Length > 0)
            {
                for (int i = 0; i < zipDirectories.Length; i++)
                {
                    string[] zipFiles = Directory.GetFiles(zipDirectories[i]);
                    if (zipFiles.Length > 0)
                    {
                        for (int j = 0; j < zipFiles.Length; j++)
                        {
                            if (objs.Length <= 1)
                            {
                                using (ZipFile zip = new ZipFile(zipDirectories[i] + ".zip"))
                                {
                                    zip.AddFile(zipFiles[j], "");
                                    zip.Save();
                                }
                            }
                            if (objs.Length > 1)
                            {
                                using (ZipFile zip = new ZipFile(zipDirectories[i] + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + ".zip"))
                                {
                                    zip.AddFile(zipFiles[j], "");
                                    zip.Save();
                                }
                            }
                        }
                    }
                }
            }

        }
    }

    /// <summary>
    /// Determine whether or not to select resources
    /// </summary>
    static void JudgeSelectPrefab(UnityEngine.Object[] objs)
    {
        isRightSelectObjs = false;
        if (objs.Length <= 0)
        {
            EditorUtility.DisplayDialog("Error", "No prefab is selected", "Close");
            return;
        }
        bool hasSameName = false; bool hasSameNameInitials = false;
        string fsame = string.Empty;
        Hashtable t = new Hashtable(objs.Length); Hashtable t1 = new Hashtable(objs.Length);
        for (int i = 0; i < objs.Length; i++)
        {
            if (Directory.Exists(AssetDatabase.GetAssetPath(objs[i])) ||
                (AssetDatabase.GetAssetPath(objs[i]).Substring(AssetDatabase.GetAssetPath(objs[i]).LastIndexOf(".")) != ".prefab"))
            {
                EditorUtility.DisplayDialog("Error", "Please select the prefab file", "Close");
                return;
            }

            if (t.Contains(objs[i].name))
            {
                hasSameName = true;
                fsame = AssetDatabase.GetAssetPath(objs[i]);
                break;
            }
            else
            {
                t.Add(objs[i].name, null);
            }
            if (t1.Contains(objs[i].name.ToLower()))
            {
                hasSameNameInitials = true;
                fsame = AssetDatabase.GetAssetPath(objs[i]);
                break;
            }
            else
            {
                t1.Add(objs[i].name.ToLower(), null);
            }
        }
        if (hasSameName)
        {
            EditorUtility.DisplayDialog("Error", "Prefab name conflicts", "Close");
            return;
        }
        if (hasSameNameInitials)
        {
            EditorUtility.DisplayDialog("Error", "Prefab name conflicts, case-insensitives", "Close");
            return;
        }
        isRightSelectObjs = true;
    }

    /// <summary>
    /// Complete the package and delete unnecessary files
    /// </summary>
    static void DeletFile(string path)
    {
        try
        {
            if (path != string.Empty)
            {
                string[] deleteFilePath = Directory.GetFiles(path);
                for (int i = 0; i < deleteFilePath.Length; i++)
                {
                    File.Delete(deleteFilePath[i]);
                }
                FileUtil.DeleteFileOrDirectory(path);
            }
        }
        catch (Exception ex)
        {
            string message = ex.Message + " Directory cannot be found. Please select again";
            Debug.LogError(message);
            Debug.LogException(ex);
            isErrorEx = true;
            return;
        }
    }

    /// <summary>
    /// The file directory that deletes the storage resource after the package has been completed
    /// </summary>
    static void DeletAssetbundleDirectory(string outpath, UnityEngine.Object[] objs)
    {
        if (objs.Length <= 1)
        {
            outpath = Path.GetDirectoryName(outpath);
        }
        if (outpath != string.Empty)
        {
            string[] deleteDirectoryPath = Directory.GetDirectories(outpath);
            if (deleteDirectoryPath.Length > 0)
            {
                for (int i = 0; i < deleteDirectoryPath.Length; i++)
                {
                    DeletFile(deleteDirectoryPath[i]);
                }
            }
            for (int i = 0; i < Directory.GetFiles(outpath).Length; i++)
            {
                if (Path.GetExtension(Directory.GetFiles(outpath)[i]) != ".zip")
                {
                    File.Delete(Directory.GetFiles(outpath)[i]);
                }
            }
        }
        Debug.Log("Finish build");
    }

    /// <summary>
    /// Pack event
    /// </summary>
    /// <param name="objs"></param>
    /// <param name="platform"></param>
    /// <param name="platformStr"></param>
    static void PackageAssetbundleEvent(UnityEngine.Object[] objs, BuildTarget platform, string platformStr)
    {
        outpath = againOutpath;
        if (outpath != string.Empty)
        {
            if (objs.Length > 0)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(objs[i]));
                    ai.assetBundleName = AssetDatabase.GetAssetPath(objs[i]);
                    string assetsInFile = ai.assetBundleName; ;
                }
                var assetbundle_ws = BuildPipeline.BuildAssetBundles(outpath, BuildAssetBundleOptions.None, platform);
                for (int i = 0; i < objs.Length; i++)
                {
                    AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(objs[i]));
                    ai.assetBundleName = null;
                }
                if (assetbundle_ws != null)
                {
                    DeletFiles(outpath, "manifest");
                    MoveFile(outpath, "prefab", objs, movFilePath, platform);
                }
            }
        }
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// According to the name of the package resource, create the appropriate directory and return all the directories below the path
    /// </summary>
    static List<DirectoryInfo> CreatePrefabDirectory(UnityEngine.Object[] objs)
    {
        List<DirectoryInfo> movFilePath = new List<DirectoryInfo>(); outpath = againOutpath;

        if (outpath != string.Empty)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                var rootPathDir = Directory.CreateDirectory(outpath + "/" + objs[i].name);
                movFilePath.Add(rootPathDir);
            }
        }
        return movFilePath;
    }

    /// <summary>
    ///Find some type of file, including subdirectories, returns an array
    /// </summary>
    /// <param name="arge"></param>
    static string[] FindAllChildDirectoreFiles(string outpath, string arge)
    {
        return Directory.GetFiles(outpath, "*." + arge, SearchOption.AllDirectories);
    }

    /// <summary>
    /// Delete all files of a certain type
    /// </summary>
    /// <param name="arge"></param>
    static void DeletFiles(string outpath, string arge)
    {
        string[] flies = FindAllChildDirectoreFiles(outpath, arge);
        for (int i = 0; i < flies.Length; i++)
        {
            File.Delete(flies[i]);
        }
    }
    /// <summary>
    /// MoveFile
    /// </summary>
    static void MoveFile(string outpath, string arge, UnityEngine.Object[] objs, List<DirectoryInfo> movFilePath, BuildTarget platform)
    {
        outpath = againOutpath; string path = "";
        string[] files = FindAllChildDirectoreFiles(outpath + "/assets", arge);

        if (files.Length != movFilePath.Count && isContinueMovFile)
        {
            isContinueMovFile = false; return;
        }
        if (files.Length == movFilePath.Count)
        {
            for (int i = 0; i < movFilePath.Count; i++)
            {
                path = outpath + "/" + movFilePath[i];
                string[] files1 = FindAllChildDirectoreFiles(outpath + "/assets", arge);
                for (int j = 0; j < files1.Length; j++)
                {
                    string sourceFileName = files1[j];
                    if (Path.GetFileName(path).ToLower() ==
                        Path.GetFileName(sourceFileName).Substring(0, Path.GetFileName(sourceFileName).Length - 7).ToLower())
                    {
                        if (platform == BuildTarget.Android)
                        {
                            File.Delete(path + "/android_5.5.assetbundle");
                            File.Move(sourceFileName, path + "/android_5.5.assetbundle");
                        }
                        if (platform == BuildTarget.iOS)
                        {
                            File.Delete(path + "/ios_5.5.assetbundle");
                            File.Move(sourceFileName, path + "/ios_5.5.assetbundle");
                        }
                    }
                }
            }
            isContinueMovFile = true;
        }
    }

    /// <summary>
    ///Sets the name of zip when packaged individually
    /// </summary>
    static void SetSingleZipName(string outPath, UnityEngine.Object[] objs)
    {
        if (outpath != string.Empty)
        {
            if (objs.Length == 1)
            {
                string zipNameDirectory = Path.GetDirectoryName(outpath);
                string[] zipNameFiles = Directory.GetFiles(outpath);
                string[] moveDirDirectoes = Directory.GetDirectories(outpath);
                string moveDir = zipNameDirectory + "ws";
                for (int i = 0; i < zipNameFiles.Length; i++)
                {
                    File.Delete(zipNameFiles[i]);
                }
                for (int i = 0; i < moveDirDirectoes.Length; i++)
                {
                    Directory.Move(moveDirDirectoes[i], moveDir);
                }
                Directory.Delete(outpath);
                Directory.Move(moveDir, zipNameDirectory + "/" + Path.GetFileNameWithoutExtension(outpath));
                againOutpath = Path.GetDirectoryName(outpath);
            }
        }
    }

    /// <summary>
    /// Determine the name of the prefab before packing
    /// </summary>
    static void SetPrefabAssetBundleName(UnityEngine.Object[] objs)
    {
        string[] prefabFiles = FindAllChildDirectoreFiles(Application.dataPath, "prefab");
        for (int i = 0; i < prefabFiles.Length; i++)
        {
            string rootNameStr = Application.dataPath.Substring(0, Application.dataPath.Length - 6);

            string aiNameStr = prefabFiles[i].Substring(rootNameStr.Length, prefabFiles[i].Length - rootNameStr.Length);

            AssetImporter ai = AssetImporter.GetAtPath(aiNameStr);

            ai.assetBundleName = null;
        }
    }


    [MenuItem("RAVVAR/RAVVAR Platform")]
    static void DarCreatorPlatform()
    {
        Application.OpenURL("https://cloud.ravvar.cn");
    }

    [MenuItem("RAVVAR/Tutorial")]
    static void Tutorial()
    {
       Application.OpenURL("https://ravvar.cn/helps");
    }

}

