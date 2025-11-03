using UnityEngine;
using TMPro;

public class ChangePuppetName : MonoBehaviour
{

    public TMP_Dropdown nameDropdown;

    public GameObject playerObject;

    // Characterlist
    private string[] puppetNames = { "April", "Flagellica", "SquimBaba", "Henbolone" };

    void Start()
    {

        nameDropdown.ClearOptions();

        nameDropdown.AddOptions(new System.Collections.Generic.List<string>(puppetNames));

        nameDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    void OnDropdownChanged(int index)
    {
        string selectedName = puppetNames[index];

        var control = playerObject.GetComponent<MarionetteControl>();
        if (control != null)
        {
            control.puppetName = selectedName;
            Debug.Log("Character change to: " + selectedName);
        }
        else
        {
            Debug.LogWarning("Cannot find MarionetteControl Script£¡");
        }
    }
}


