using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;
using SFB;
public class ApplicationWindow : MonoBehaviour
{
    
    public Text infoText;
    TextAsset infoAppList;
    TextAsset infoEditApp;

    public GameObject applicationListWindow;
    public GameObject newEditApplicationWindow;
    public Text newEditApplicationText;

    public TMP_InputField appNameField;
    public TMP_InputField appCMDField;
    public TMP_InputField appDetachedField;

    public GameObject appTemplate;

    public List<AppData> apps;

    [System.Serializable]
    public class AppData
    {
        public string name;
        public string cmd;
        public string detached;
    }

    private void OnEnable()
    {
        LoadJSONFile();

        infoAppList = Resources.Load<TextAsset>("infoapplist");
        infoEditApp = Resources.Load<TextAsset>("infoeditapp");

        SwitchToAppList();
    }

    int editID;
    bool isEdit;
    public void SwitchToNewApp()
    {
        isEdit = false;
        newEditApplicationText.text = "ADD A NEW APPLICATION";
        applicationListWindow.SetActive(false);
        newEditApplicationWindow.SetActive(true);

        appNameField.text = "";
        appCMDField.text = "";
        appDetachedField.text = "";

        infoText.text = infoEditApp.text;
    }

    public void SwitchToEditApp(int id)
    {
        editID = id;
        isEdit = true;
        newEditApplicationText.text = "EDIT APPLICATION";
        applicationListWindow.SetActive(false);
        newEditApplicationWindow.SetActive(true);

        appNameField.text = apps[editID].name;
        appCMDField.text = apps[editID].cmd;
        appDetachedField.text = apps[editID].detached;

        infoText.text = infoEditApp.text;
    }

    public void SwitchToAppList()
    {
        applicationListWindow.SetActive(true);
        newEditApplicationWindow.SetActive(false);

        infoText.text = infoAppList.text;
    }

    public void AddEditNewAppNow()
    {
        if (!isEdit) {
            AppData newApp = new AppData();
            newApp.name = appNameField.text;
            newApp.cmd = appCMDField.text.Normalize();
            newApp.detached = appDetachedField.text;
            apps.Add(newApp);
        }
        else
        {
            apps[editID].name = appNameField.text;
            apps[editID].cmd = appCMDField.text;
            apps[editID].detached = appDetachedField.text;
        }
        SaveJSONFile();
        SwitchToAppList();
    }

    public void OpenEXE()
    {
        appCMDField.text = StandaloneFileBrowser.OpenFilePanel("Open File", "", "exe", false)[0];
    }

    public void DeleteApp( int id)
    {
        apps.RemoveAt(id);
        SaveJSONFile();
    }

    void LoadJSONFile()
    {

        string jsonString = File.ReadAllText(@"C:\Users\MaximumSettings\apps_windows.json");
        print(jsonString);
        apps = new List<AppData>();

        string[] rawApps = Regex.Split(jsonString, "\"apps\":")[1].Split("\n"[0]);//jsonString.Split("\"apps\": ["[0])[1];

        int curListID = -1;
        for (int i = 0; i < rawApps.Length; i++)
        {
            if (rawApps[i].Contains("\"name\""))
            {
                curListID += 1;
                apps.Add(new AppData());
                apps[curListID].name =  rawApps[i].Split("\""[0])[3];
            }
            if (rawApps[i].Contains("\"cmd\"")) apps[curListID].cmd =  rawApps[i].Split("\""[0])[3].Replace("/", "\\").Replace("\\\\", "\\");
            if (rawApps[i].Contains("\"detached\"")) apps[curListID].detached = rawApps[i + 0].Split("\""[0])[3];
        }

        for (int i = 1; i < appTemplate.transform.parent.childCount; i++)
            Destroy(appTemplate.transform.parent.GetChild(i).gameObject);

        appTemplate.SetActive(false);
        for (int i = 0; i < apps.Count; i++)
        {
            Transform newApp = Instantiate(appTemplate.transform, appTemplate.transform.parent);
            newApp.gameObject.SetActive(true);
            newApp.Find("Text").GetComponent<Text>().text = apps[i].name;
            newApp.GetComponent<AppListObject>().appID = i;
        }
    }

    void SaveJSONFile()
    {
        TextAsset template = Resources.Load<TextAsset>("app-template");

        string newApp = "";
        for (int i = 0; i < apps.Count; i++)
        {
            newApp = newApp + "{ \n \"name\": \"" + apps[i].name + "\",";
            if (apps[i].detached != null && apps[i].detached != "" && apps[i].detached.Length > 2) newApp = newApp + "\n \"detached\": [ \"" + apps[i].detached + "\" ],";
            
            if(apps[i].cmd.Contains("\\"))
            newApp = newApp + "\n \"cmd\": \"" + apps[i].cmd.Replace("\\", "\\\\") + "\" \n }";
            else newApp = newApp + "\n \"cmd\": \"" + apps[i].cmd + "\" \n }";
            // newApp = newApp + "\n \"output\":\"" + "" + "\" \n }";

            if (i != apps.Count - 1) newApp = newApp + ", \n";

        }

        string result = template.text.Replace("#", newApp);
        //print(result);
        File.WriteAllText(@"C:\Users\MaximumSettings\apps_windows.json", result);

        LoadJSONFile();
    }
}
