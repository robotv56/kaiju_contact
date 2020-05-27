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

            //make sure something was entered
            if (!(inputText.Length > 0)) valid = false;
            
            //make sure theres only numbers and periods
            if(valid)
            {
                foreach (char c in array)
                {
                    if ((!char.IsDigit(c) && c != '.'))
                    {
                        log.text += ("\n \"" + inputText + "\" is not a valid IP adress");
                        valid = false;
                        break;
                    }
                }
            }

            //if all checks pass, attempt to join
            if (valid)
            {
                log.text += ("\n Attempting to join " + inputText + "...");
            }
        }

        if(log.isTextTruncated)
        {
            int i = 0;

            foreach(char c in log.text.ToCharArray())
            {
                i++;
                if(c.Equals('\n'))
                {
                    break;
                }
            }

            string buffer = log.text;
            buffer = buffer.Substring(i);
            log.SetText(buffer);
        }
    }
}
