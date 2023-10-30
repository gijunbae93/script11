using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IDestoryUiFader { void DestoryUiFader(); } // public access for destroying imagefader
public class UiFader : IDestoryUiFader // handles alpha value animation for image and tmpugui
{
    Image image = null;
    TextMeshProUGUI textMeshProUGUI = null;
    float startAlphaValue;
    float endAlphaValue;
    float duration;
    float timeElapsed = 0;
    float startDelay; // timer for starting image fader so if this is set to 2 image fading starts 2 seconds after
    Action OnComplete;
    float delayBetweenOnComplete; // used when fading animation finishes delay on invoking oncomplete action, if this is set 2 when image fading animation is complete after 2 second Oncomplete action will be invoked

    GameObject uiFaderMonobehaviour;
    bool isDestroyed;

    public static GameObject uiFaderGameObject { private get; set; } // need to be set null upon exit game, purpose of this game obejct is to set imagefadermonobehaviour to child, more cleaner hierarcy

    public static IDestoryUiFader CreateImageFader(Image image, float startAlphaValue, float endAlphaValue, float duration, float startDelay = 0, Action OnComplete = null, float delayBetweenOnComplete = 0)
    {
        if (duration <= 0 || startDelay < 0 || delayBetweenOnComplete < 0) Debug.Log("Error"); // they must be positive values

        IDestoryUiFader destroyImageFader = new UiFader(image, startAlphaValue, endAlphaValue, duration, startDelay, OnComplete, delayBetweenOnComplete);
        return destroyImageFader;
    }
    public static IDestoryUiFader CreateTMPUGUIFader(TextMeshProUGUI textMeshProUGUI, float startAlphaValue, float endAlphaValue, float duration, float startDelay = 0, Action OnComplete = null, float delayBetweenOnComplete = 0)
    {
        if (duration <= 0 || startDelay < 0 || delayBetweenOnComplete < 0) Debug.Log("Error"); // they must be positive values

        IDestoryUiFader destroyImageFader = new UiFader(textMeshProUGUI, startAlphaValue, endAlphaValue, duration, startDelay, OnComplete, delayBetweenOnComplete);
        return destroyImageFader;
    }

    public class UiFaderMonobehaviour : MonoBehaviour
    {
        UiFader imageFadeHandler;
        public void InitalizeImageFadeHandlerMonobehaviour(UiFader imageFadeHandler) => this.imageFadeHandler = imageFadeHandler;

        void Update() { if(imageFadeHandler != null) imageFadeHandler.Update(); }
    }

    UiFader(Image image, float startAlphaValue, float endAlphaValue, float duration, float startDelay, Action OnComplete, float delayBetweenOnComplete) // used for image fader
    {
        this.image = image;
        this.startAlphaValue = startAlphaValue;
        this.endAlphaValue = endAlphaValue;
        this.duration = duration;
        this.startDelay = startDelay;
        this.OnComplete = OnComplete;
        this.delayBetweenOnComplete = delayBetweenOnComplete;

        Initalize();

        uiFaderMonobehaviour = new GameObject("UiFadeHandlerMonobehaviour", typeof(UiFaderMonobehaviour));
        uiFaderMonobehaviour.GetComponent<UiFaderMonobehaviour>().InitalizeImageFadeHandlerMonobehaviour(this);
        uiFaderMonobehaviour.transform.SetParent(uiFaderGameObject.transform);
    }
    UiFader(TextMeshProUGUI textMeshProUGUI, float startAlphaValue, float endAlphaValue, float duration, float startDelay, Action OnComplete, float delayBetweenOnComplete) // used for textmeshprougui fader
    {
        this.textMeshProUGUI = textMeshProUGUI;
        this.startAlphaValue = startAlphaValue;
        this.endAlphaValue = endAlphaValue;
        this.duration = duration;
        this.startDelay = startDelay;
        this.OnComplete = OnComplete;
        this.delayBetweenOnComplete = delayBetweenOnComplete;

        Initalize();

        uiFaderMonobehaviour = new GameObject("UiFadeHandlerMonobehaviour", typeof(UiFaderMonobehaviour));
        uiFaderMonobehaviour.GetComponent<UiFaderMonobehaviour>().InitalizeImageFadeHandlerMonobehaviour(this);
        uiFaderMonobehaviour.transform.SetParent(uiFaderGameObject.transform);
    }

    void Initalize() => uiFaderGameObject = uiFaderGameObject ?? new GameObject("imageFadeHandlerGameObject"); // imageFadeHandlerGameObject 

    // Update is called once per frame
    public void Update()
    {
        if (isDestroyed) return;

        Color uiColor = image == null ? textMeshProUGUI.color : image.color;

        startDelay -= Time.deltaTime;
        if(startDelay <= 0)
        {
            if (timeElapsed < duration)
            {
                uiColor.a = Mathf.Lerp(startAlphaValue, endAlphaValue, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
            }
            else
            {
                uiColor.a = endAlphaValue;
                
                if(delayBetweenOnComplete <= 0) // delayBetweenOnComplete if this was not set it will just ignore this and run below action and function
                {
                    DestoryUiFader();

                    if (image == null)
                        textMeshProUGUI.color = uiColor;
                    else
                        image.color = uiColor;

                    // if DestoryImageFader was invoked during fading animation this will not be invoked becuase isDestroyed will become true and update function will return,
                    // this have to be invoked after uifader have been destroyed or it will have conflict with other fader or alpha change
                    OnComplete?.Invoke();

                    return; // if i dont return if i set alpha value of same image or textmeshpro right away (ex. setting alpha in oncomplete) it will have problem because in below it will set alpha
                }
            }

            if (image == null)
                textMeshProUGUI.color = uiColor;
            else
                image.color = uiColor;
        }

        if (uiColor.a == endAlphaValue && delayBetweenOnComplete > 0) delayBetweenOnComplete -= Time.deltaTime;
    }

    public void DestoryUiFader()
    {
        if (isDestroyed) return; // can be called again if reference of interface exist

        GameObject.Destroy(uiFaderMonobehaviour);

        isDestroyed = true;
    }
}
