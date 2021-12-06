using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HostSettingsWindow : MonoBehaviour
{
    public TMP_Dropdown encoderDropdown;
    public TMP_Dropdown gamepadDropdown;
    public ToggleController altKeyToggle;
    public TMP_InputField swPresetField;
    public TMP_InputField swTuneField;
    public Slider fecSlider;

    public TMP_Dropdown encoderTypeDropdown;
    public Transform softwareEncodingArguments;
    public Transform encoderPreferences;

    public string controllerType = "x360";
    public string amfPreset = "speed";
    public string nvencPreset = "default";

    public string[] nvencPresetNames = new string[] { "Default", "High Performance", "High Quality", "Lossless" };
    public string[] nvencPresets = new string[] { "default", "hp", "hq", "lossless" };

    public string[] amfPresetNames = new string[] { "Speed", "Quality", "Balanced" };
    public string[] amfPresets = new string[] { "speed", "default", "balanced" };

    public string encoderTypeString = "software";

    public GPUType gpuType;
    public enum GPUType { NVIDIA,AMD};

    public int fecPercent = 20;
    public TMP_Dropdown swPresetDropdown;
    public string swPreset = "superfast";
    public string swTune = "zerolatency";

    private void Awake()
    {
        string gpuName = SystemInfo.graphicsDeviceName.ToUpper();
        if (gpuName.Contains("NVIDIA") || gpuName.Contains("GTX") || gpuName.Contains("QUADRO"))
        {
            gpuType = GPUType.NVIDIA;
            encoderDropdown.transform.parent.Find("Text").GetComponent<Text>().text = "Nvidia Encoding Preference";
        }
        else
        {
            gpuType = GPUType.AMD;
            encoderDropdown.transform.parent.Find("Text").GetComponent<Text>().text = "AMD Encoding Preference";
        }

        LoadConfigFile();

    }

    public void OnEncoderTypeChange(int newID)
    {
        if (newID == 0)
        {
            softwareEncodingArguments.gameObject.SetActive(false);
            encoderPreferences.gameObject.SetActive(true);
            if (gpuType == GPUType.AMD) encoderTypeString = "amdvce";
            if (gpuType == GPUType.NVIDIA) encoderTypeString = "nvenc";
        }
        else
        {
            softwareEncodingArguments.gameObject.SetActive(true);
            encoderPreferences.gameObject.SetActive(false);
            encoderTypeString = "software";
        }
    }


    string configString;
    int curPresetID;
    bool hasAMDCoder;
    bool hasSWPreset;
    bool hasSWTune;
    bool hasFEC;
    bool hasEncoderType;
    void LoadConfigFile()
    {
        configString = File.ReadAllText(@"C:\Users\MaximumSettings\sunshine.conf");
        curPresetID = 0;

        hasAMDCoder = false;
        hasEncoderType = false;
        string[] rawData = Regex.Split(configString, "\n");

        for (int i = 0; i < rawData.Length; i++)
        {
            if (rawData[i].Contains("nv_preset") && gpuType == GPUType.NVIDIA)
                nvencPreset = Regex.Split(rawData[i], "=")[1].Replace(" ", "");
            if (rawData[i].Contains("amd_quality") && gpuType == GPUType.AMD)
                amfPreset = Regex.Split(rawData[i], "=")[1].Replace(" ", "");

            if (rawData[i].Contains("gamepad"))
                controllerType = Regex.Split(rawData[i], "=")[1].Replace(" ", "");

            //key_rightalt_to_key_win
            if (rawData[i].Contains("key_rightalt_to_key_win"))
            {
                string useRightAlt = Regex.Split(rawData[i], "=")[1].Replace(" ", "");
                if (useRightAlt.Contains("disabled")) altKeyToggle.isOn = false;
                else altKeyToggle.isOn = true;
            }

            if (rawData[i].Contains("amd_coder") && gpuType == GPUType.AMD) hasAMDCoder = true;

            if (rawData[i].Contains("sw_preset"))
            {
                hasSWPreset = true;
                swPreset = Regex.Split(rawData[i], "=")[1].Replace(" ", "");

                for (int ii = 0; ii < swPresetDropdown.options.Count; ii++)
                {
                    if (swPreset.ToUpper() == swPresetDropdown.options[ii].text.ToUpper())
                        swPresetDropdown.value = ii;
                }
            }
            if (rawData[i].Contains("sw_tune"))
            {
                hasSWTune = true;
                swTune = Regex.Split(rawData[i], "=")[1].Replace(" ", "");
            }
            if (rawData[i].Contains("fec_percentage"))
            {
                hasFEC = true;
                int.TryParse(Regex.Split(rawData[i], "=")[1].Replace(" ", ""), out fecPercent);
            }
            if (rawData[i].Contains("encoder"))
            {
                hasEncoderType = true;
                encoderTypeString = Regex.Split(rawData[i], "=")[1].Replace(" ", "");
                if (encoderTypeString.Contains("software"))
                {
                    encoderTypeDropdown.value = 1;
                    OnEncoderTypeChange(1);
                }
            }
        }

        if (!hasAMDCoder) configString = configString + "\namd_rc = 0\namd_coder = auto\n";

        if (!hasSWPreset) { configString = configString + "\nsw_preset = superfast\nsw_tune = zerolatency\n";
            swPresetDropdown.value = 1;
        }

        if(!hasFEC) configString = configString + "\nfec_percentage = 20\n";

        if (!hasEncoderType)
        {
            if (gpuType == GPUType.AMD) encoderTypeString = "amdvce";
            if (gpuType == GPUType.NVIDIA) encoderTypeString = "nvenc";
            configString = configString + "\nencoder = "+encoderTypeString+"\n";
        }


        if (controllerType.Contains("ds4"))
            gamepadDropdown.value = 1;
        else gamepadDropdown.value = 0;

        encoderDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> dropdowns = new List<TMP_Dropdown.OptionData>();
        if(gpuType == GPUType.NVIDIA)
        {
            for (int i = 0; i < nvencPresetNames.Length; i++)
            {
                TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData();
                newOption.text = nvencPresetNames[i];
                dropdowns.Add(newOption);
                if (nvencPreset.Contains(nvencPresets[i])) curPresetID = i;
            }
        }
        else
        {
            for (int i = 0; i < amfPresetNames.Length; i++)
            {
                TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData();
                newOption.text = amfPresetNames[i];
                dropdowns.Add(newOption);
                if (amfPreset.Contains(amfPresets[i])) curPresetID = i;
            }
        }

        encoderDropdown.AddOptions(dropdowns);
        encoderDropdown.value = curPresetID;

        swPresetField.text = swPreset;
        swTuneField.text = swTune;
        fecSlider.value = fecPercent;
    }

    public void SaveConfigFile()
    {
        string[] rawData = Regex.Split(configString, "\n");

        if (gpuType == GPUType.NVIDIA)
            nvencPreset = nvencPresets[encoderDropdown.value];
        if (gpuType == GPUType.AMD)
            amfPreset = amfPresets[encoderDropdown.value];

        swPreset = swPresetDropdown.options[swPresetDropdown.value].text;//swPresetField.text;
        swTune = swTuneField.text;
        fecPercent = (int)fecSlider.value;

        for (int i = 0; i < rawData.Length; i++)
        {
            if (rawData[i].Contains("nv_preset") && gpuType == GPUType.NVIDIA)
                rawData[i] = rawData[i].Split("="[0])[0] + "= " + nvencPreset;
            if (rawData[i].Contains("amd_quality") && gpuType == GPUType.AMD)
                rawData[i] = rawData[i].Split("="[0])[0] + "= " + amfPreset;

            if (rawData[i].Contains("gamepad"))
            {
                if (gamepadDropdown.value == 0) controllerType = "x360";
                if (gamepadDropdown.value == 1) controllerType = "ds4";
                rawData[i] = rawData[i].Split("="[0])[0] + "= " + controllerType;
            }

            if (rawData[i].Contains("key_rightalt_to_key_win"))
            {
                string rightalt = "disabled";
                if (altKeyToggle.isOn) rightalt = "enabled";
                rawData[i] = rawData[i].Split("="[0])[0] + "= " + rightalt;
            }

            if (rawData[i].Contains("sw_preset"))
                rawData[i] = rawData[i].Split("="[0])[0] + "= " + swPreset;

            if (rawData[i].Contains("sw_tune"))
                rawData[i] = rawData[i].Split("="[0])[0] + "= " + swTune;

            if (rawData[i].Contains("fec_percentage"))
                rawData[i] = rawData[i].Split("="[0])[0] + "= " + fecPercent.ToString();

            if (rawData[i].Contains("encoder"))
                rawData[i] = rawData[i].Split("="[0])[0] + "= " + encoderTypeString;
        }

        string result = "";
        for (int i = 0; i < rawData.Length; i++)
            if(rawData[i].Length > 1)result = result + rawData[i] + "\n";

       File.WriteAllText(@"C:\Users\MaximumSettings\sunshine.conf", result);
    }
}
