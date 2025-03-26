using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class LEDVideo : MonoBehaviour
{
    [Header("LED Size")] 
    [SerializeField] private Renderer renderer;
    [SerializeField] private FloatPair baseSize;

    [SerializeField] private FloatPair baseDensity;
    
    [Header("Video Clip")]
    [SerializeField] private List<VideoClip> videoClips = new List<VideoClip>();
    [SerializeField] private VideoPlayer videoPlayer;
    
    private int currentClipIndex = 0;

    private void Start()
    {
        this.transform.localScale = new Vector3(baseSize.x, baseSize.y, 1);
        Material mat  = renderer.material;
        mat.SetFloat("TilingLEDX", baseDensity.x);
        mat.SetFloat("TilingLEDY", baseDensity.y);
    }

    public void SetVideoClip(int index)
    {
        if (index == currentClipIndex)
            return;
        
        if(index >= videoClips.Count)
            return;
        
        currentClipIndex = index;
        videoPlayer.clip = videoClips[index];
        videoPlayer.Play();
        videoPlayer.isLooping = true;
    }

    public void ChangeScreenSize(int sizeFactor)
    {
        float multiplier = Mathf.Pow(2f, sizeFactor);
        float width = baseSize.x * multiplier;
        float height = baseSize.y * multiplier;
        float xDensity = baseDensity.x * multiplier;
        float yDensity = baseDensity.y * multiplier;
        
        this.transform.localScale = new Vector3(width, height, 1);
        
        Material mat  = renderer.material;
        mat.SetFloat("TilingLEDX", xDensity);
        mat.SetFloat("TilingLEDY", yDensity);
    }
}

[System.Serializable]
public struct FloatPair
{
    public float x;
    public float y;

    public FloatPair(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}
