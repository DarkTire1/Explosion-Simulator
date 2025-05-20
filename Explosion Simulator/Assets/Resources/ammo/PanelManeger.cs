using UnityEngine;
using TMPro; 

public class PanelManager : MonoBehaviour
{
    public GameObject panel;
    public TMP_InputField inputField1;
    public TMP_InputField inputField2; 
    public TMP_InputField inputField3;
    public TMP_InputField inputField4; 


    public string defaultValue1 = "";
    public string defaultValue2 = "";
    public string defaultValue3 = "";
    public string defaultValue4 = "";

    public void TogglePanel()
    {
        if (panel != null)
        {
            bool isActive = panel.activeSelf; 
            panel.SetActive(!isActive);
            if (inputField1 != null)
            {
                SetDefaultInputValues();
            }
           
        }
    }

    private void SetDefaultInputValues()
    {
        if (inputField1 != null) inputField1.text = defaultValue1;
        if (inputField2 != null) inputField2.text = defaultValue2;
        if (inputField3 != null) inputField3.text = defaultValue3;
        if (inputField4 != null) inputField4.text = defaultValue4;
    }
}
