using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GPTDialogue : MonoBehaviour
{
    private TMPro.TextMeshProUGUI _textComponent;

    //*** Flexible Textbox for taskmessage
    private RectTransform _HGroupTaskMessage;

    private bool _isFading;

    // Start is called before the first frame update
    public void Init()
    {
        HorizontalLayoutGroup temp = gameObject.GetComponentInChildren<HorizontalLayoutGroup>();
        //init task message group
        RectTransform _HGroupTaskMessage = temp.gameObject.GetComponent<RectTransform>();
        TMPro.TextMeshProUGUI[] allText = _HGroupTaskMessage.gameObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>();

        _textComponent = allText[0];
        _textComponent.text = "";
    }

    public void Update()
    {
        var lookPos = transform.position - AngelARUI.Instance.ARCamera.transform.position;
        lookPos.y = 0;
        transform.rotation = Quaternion.LookRotation(lookPos, Vector3.up);

        if (!_isFading && EyeGazeManager.Instance.CurrentHit.Equals(EyeTarget.gptDialogue))
        {
            _isFading = true;
            StartCoroutine(FadeGPTDialogue());
        }
    }

    private IEnumerator FadeGPTDialogue()
    {
        float counter = 15;
        while (_isFading && counter>0)
        {
            counter -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (counter<=0)
            Orb.Instance.SetDialogueActive(false);

        _isFading = false;
    }

    public void OnDisable()
    {
        _isFading = false;
        StopCoroutine(FadeGPTDialogue());
    }

    public void SetText(string utterance, string response)
    {
        string res_short = Utils.SplitTextIntoLines(response, ARUISettings.OrbMessageMaxCharCountPerLine);

        if (utterance.Length==0)
        {
            _textComponent.text = res_short;
        } else
        {
            string utt_short = Utils.SplitTextIntoLines(utterance, ARUISettings.OrbMessageMaxCharCountPerLine);

            _textComponent.text = "<b>You:</b> " + utt_short + "\n\n" + "<b>Angel:</b> " + res_short;
        }
    }
}
