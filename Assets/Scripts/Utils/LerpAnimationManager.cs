using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class LerpAnimationManager : MonoBehaviour
{
    private static LerpAnimationManager _instance = null;
    public static LerpAnimationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LerpAnimationManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("LerpAnimationManager");
                    _instance = obj.AddComponent<LerpAnimationManager>();
                }
            }
            return _instance;
        }
    }

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private List<LerpAnimation> activeAnimations = new List<LerpAnimation>();
    private List<LerpAnimation> animsToAdd = new List<LerpAnimation>();
    private List<Guid> animsToRemove = new List<Guid>();
    private List<int> avialbleIndices = new List<int>();
    int length = 0;
    void Update()
    {
        if (animsToAdd.Count > 0)
        {
            activeAnimations.AddRange(animsToAdd);
            animsToAdd.Clear();
        }
        
        if (activeAnimations.Count == 0)
            return;
        length = activeAnimations.Count;
        for (int i = length - 1; i >= 0; i--)
        {
            // Debug.Log($"Updating animation {i} length: {activeAnimations.Count}");
            var anim = activeAnimations[i];
            if (animsToRemove.Contains(anim.id))
            {
                animsToRemove.Remove(anim.id);
                // activeAnimations.RemoveAt(i);
                anim.onComplete?.Invoke();
                anim.isActive = false;
                activeAnimations[i] = anim;
                avialbleIndices.Add(i);
                continue;
            }
            if (anim.isActive == false)
            {
                continue;
            }
            float t = Mathf.Clamp01((Time.realtimeSinceStartup - anim.startTime) / anim.duration);

            // Scale
            if (anim.targetTransform && anim.lerpScale)
                anim.targetTransform.localScale = Vector3.Lerp(anim.startScale, anim.endScale, t);

            // Color
            if (anim.lerpColor)
            {
                if (anim.targetSpriteRenderer)
                    anim.targetSpriteRenderer.color = Color.Lerp(anim.startColor, anim.endColor, t);
                else if (anim.targetImage)
                    anim.targetImage.color = Color.Lerp(anim.startColor, anim.endColor, t);
                else if (anim.targetRawImage)
                    anim.targetRawImage.color = Color.Lerp(anim.startColor, anim.endColor, t);
            }

            // Rotation
            if (anim.targetTransform && anim.lerpRotation)
                anim.targetTransform.localRotation = Quaternion.Lerp(anim.startRotation, anim.endRotation, t);

            // Position
            if (anim.targetTransform && anim.lerpPosition)
                anim.targetTransform.localPosition = Vector3.Lerp(anim.startPosition, anim.endPosition, t);

            if (t >= 1f)
            {
                // if (activeAnimations.Count == 1)
                //     activeAnimations.Clear();
                // else if (i < activeAnimations.Count)
                anim.onComplete?.Invoke();
                // activeAnimations.RemoveAt(i);
                anim.isActive = false;  
                activeAnimations[i] = anim;
                avialbleIndices.Add(i);
            }
            else
            {
                activeAnimations[i] = anim;
            }
        }
    }

    public LerpAnimation StartMoveLerp(
        Transform target,
        Vector3 positionFrom,
        Vector3 positionTo,
        float lerpDuration = 0.5f,
        Action onComplete = null)
    {
        return StartLerp(
            target,
            positionFrom: positionFrom,
            positionTo: positionTo,
            lerpDuration: lerpDuration,
            onComplete: onComplete);
    }
    public LerpAnimation StartScaleLerp(
        Transform target,
        Vector3 scaleFrom,
        Vector3 scaleTo,
        float lerpDuration = 0.5f,
        Action onComplete = null)
    {
        return StartLerp(
            target,
            scaleFrom: scaleFrom,
            scaleTo: scaleTo,
            lerpDuration: lerpDuration,
            onComplete: onComplete);
    }
    
    public LerpAnimation StartTransformLerp(
        Transform target,
        Vector3? scaleFrom = null, Vector3? scaleTo = null,
        Quaternion? rotationFrom = null, Quaternion? rotationTo = null,
        Vector3? positionFrom = null, Vector3? positionTo = null,
        float lerpDuration = 0.5f,
        Action onComplete = null)
    {
        return StartLerp(
            target,
            scaleFrom: scaleFrom,
            scaleTo: scaleTo,
            rotationFrom: rotationFrom,
            rotationTo: rotationTo,
            positionFrom: positionFrom,
            positionTo: positionTo,
            lerpDuration: lerpDuration,
            onComplete: onComplete);
    }

    public LerpAnimation StartColorLerp(
        Transform target,
        Color colorFrom,
        Color colorTo,
        float lerpDuration = 0.5f,
        SpriteRenderer spriteRenderer = null,
        Image image = null,
        RawImage rawImage = null,
        Action onComplete = null)
    {
        return StartLerp(
            target,
            colorFrom: colorFrom,
            colorTo: colorTo,
            lerpDuration: lerpDuration,
            spriteRenderer: spriteRenderer,
            image: image,
            rawImage: rawImage,
            onComplete: onComplete);
    }

    public LerpAnimation StartLerp(
        Transform target,
        Vector3? scaleFrom = null, Vector3? scaleTo = null,
        Color? colorFrom = null, Color? colorTo = null,
        Quaternion? rotationFrom = null, Quaternion? rotationTo = null,
        Vector3? positionFrom = null, Vector3? positionTo = null,
        float lerpDuration = 0.5f,
        SpriteRenderer spriteRenderer = null,
        Image image = null,
        RawImage rawImage = null,
        Action onComplete = null)
    {   
        totalAnimSent++;
        LerpAnimation anim;
        bool haveToAddNew = false; 
        int index = -1;
        if (avialbleIndices.Count > 0)
        {
            totalAnimUsedFromIndices++;
            anim = activeAnimations[avialbleIndices[0]];
            index = avialbleIndices[0];
            avialbleIndices.RemoveAt(0);
        }
        else
        {
            totalAnimUsedFromAnimToAdd++;
            anim = new LerpAnimation();
            haveToAddNew = true;
        }
        anim.id = Guid.NewGuid(); 
        anim.targetTransform = target;
        anim.targetSpriteRenderer = spriteRenderer;
        anim.targetImage = image;
        anim.targetRawImage = rawImage;
        anim.lerpScale = scaleFrom.HasValue && scaleTo.HasValue;
        anim.lerpColor = colorFrom.HasValue && colorTo.HasValue;
        anim.lerpRotation = rotationFrom.HasValue && rotationTo.HasValue;
        anim.lerpPosition = positionFrom.HasValue && positionTo.HasValue;
        anim.startScale = scaleFrom ?? Vector3.zero;
        anim.endScale = scaleTo ?? Vector3.zero;
        anim.startColor = colorFrom ?? Color.white;
        anim.endColor = colorTo ?? Color.white;
        anim.startRotation = rotationFrom ?? Quaternion.identity;
        anim.endRotation = rotationTo ?? Quaternion.identity;
        anim.startPosition = positionFrom ?? Vector3.zero;
        anim.endPosition = positionTo ?? Vector3.zero;
        anim.duration = lerpDuration;
        anim.startTime = Time.realtimeSinceStartup;
        anim.onComplete = onComplete;
        anim.isActive = true;
        if (index != -1)
            activeAnimations[index] = anim;
        if (haveToAddNew)
            animsToAdd.Add(anim);
        return anim;
    }
    public void CancelLerp(LerpAnimation animation)
    {
        animsToRemove.Add(animation.id);
    }
    private int totalAnimSent = 0;
    private int totalAnimUsedFromIndices = 0;
    private int totalAnimUsedFromAnimToAdd = 0;

    public void ResetStat()
    {
        totalAnimSent = 0;
        totalAnimUsedFromIndices = 0;
        totalAnimUsedFromAnimToAdd = 0;
    }
    public void DebugStats()
    {
        Debug.Log($"SPLog: LerpAnimationManager Stats: totalAnimSent: {totalAnimSent}, totalAnimUsedFromIndices: {totalAnimUsedFromIndices}, totalAnimUsedFromAnimToAdd: {totalAnimUsedFromAnimToAdd}");
    }
    

    public struct LerpAnimation
    {
        public Guid id;
        public Transform targetTransform;
        public SpriteRenderer targetSpriteRenderer;
        public Image targetImage;
        public RawImage targetRawImage;
        public bool lerpScale, lerpColor, lerpRotation, lerpPosition;
        public Vector3 startScale, endScale;
        public Color startColor, endColor;
        public Quaternion startRotation, endRotation;
        public Vector3 startPosition, endPosition;
        public float duration;
        public float startTime;
        public Action onComplete;
        public bool isActive;
    }
}