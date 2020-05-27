using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JoinScreenController : MonoBehaviour
{
    public TMP_InputField field;
    public TMP_Text log;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        field.ActivateInputField();

        if(Input.GetKeyDown(KeyCode.Return))
        {
            string inputText = field.text;
            field.text = "";

            char[] array = inputText.ToCharArray();

            bool valid = true;

            foreach(char c in array)
            {
                if((!char.IsDigit(c) && c != '.'))
                {
                    Debug.Log(inputText + " is invalid");
                    log.text += ("\n \"" + inputText + "\" is not a valid IP adress");
                    valid = false;
                    break;
                } 
            }
            if (valid)
            {
                log.text += ("\n Attempting to join " + inputText + "...");
                Debug.Log(inputText + " is valid");
            }
        }
    }
}
