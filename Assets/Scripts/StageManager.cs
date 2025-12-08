using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("Stage Containers")]
    public GameObject[] stages; 
    // Order must match Dropdown options:
    // 0 = Dump
    // 1 = Forest
    // 2 = Kitchen

    [Header("UI")]
    public TMP_Dropdown stageDropdown;

private void Start()
{
    stageDropdown.ClearOptions();

    var options = new List<string>();
    foreach (var stage in stages)
        options.Add(stage.name.Replace("Stage_", ""));

    stageDropdown.AddOptions(options);

    stageDropdown.onValueChanged.AddListener(ChangeStage);
    ChangeStage(0);
}


    public void ChangeStage(int index)
    {
        // Safety check
        if (stages == null || stages.Length == 0)
            return;

        for (int i = 0; i < stages.Length; i++)
        {
            stages[i].SetActive(i == index);
        }
    }
}
