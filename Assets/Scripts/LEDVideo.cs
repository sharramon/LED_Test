using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.Video;

public class LEDVideo : MonoBehaviour
{
    [Header("LED Size")] 
    [SerializeField] private Renderer renderer;
    private Material mat;
    [SerializeField] private FloatPair baseVideoSize;
    [SerializeField] private FloatPair baseImageSize;
    [SerializeField]private float baseDensity;
    
    [Header("Video Clip")]
    [SerializeField] private List<VideoClip> videoClips = new List<VideoClip>();
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RenderTexture renderTexture;
    
    [Header("Images")]
    [SerializeField] private List<Texture> images = new List<Texture>();
    
    private int currentClipIndex = 0;
    private int currentImageIndex = -1;

    private void Start()
    {
        this.transform.localScale = new Vector3(baseVideoSize.x, baseVideoSize.y, 1);
        mat  = renderer.material;
        mat.SetFloat("TilingLEDX", baseDensity * baseVideoSize.x);
        mat.SetFloat("TilingLEDY", baseDensity * baseVideoSize.y);
    }

    public void SetVideoClip(int index)
    {
        currentImageIndex = -1; //bad workaround for image to video
        
        if (index == currentClipIndex)
            return;

        if (index >= videoClips.Count)
            return;
        
        //set video clip
        currentClipIndex = index;
        //ChangeScreenSize(LEDSceneManager.Instance.GetZoomInt());
        videoPlayer.clip = videoClips[index];
        mat.SetTexture("_BaseMap", renderTexture);
        
        //play
        videoPlayer.Play();
        videoPlayer.isLooping = true;
    }

    public void ChangeScreenSize(int sizeFactor)
    {
        float multiplier = Mathf.Pow(2f, sizeFactor);
        float width, height;
        
        if (currentClipIndex != -1) //meaning if it's currently video
        {
            width = baseVideoSize.x * multiplier;
            height = baseVideoSize.y * multiplier;
        }
        else
        {
            width = baseImageSize.x * multiplier;
            height = baseImageSize.y * multiplier;
        }
        float xDensity = baseDensity * width;
        float yDensity = baseDensity * height;
        
        this.transform.localScale = new Vector3(width, height, 1);
        
        mat.SetFloat("TilingLEDX", xDensity);
        mat.SetFloat("TilingLEDY", yDensity);
    }
    
    public void ChangeScreenSizeNormalized(float t)
    {
        // map t∈[0,1] to sizeFactor∈[–2,2]
        float sizeFactor = Mathf.Lerp(-2f, 2f, t);

        // now exactly as before
        float multiplier = Mathf.Pow(2f, sizeFactor);

        float width, height;
        if (currentClipIndex != -1)
        {
            width  = baseVideoSize.x * multiplier;
            height = baseVideoSize.y * multiplier;
        }
        else
        {
            width  = baseImageSize.x * multiplier;
            height = baseImageSize.y * multiplier;
        }

        transform.localScale = new Vector3(width, height, 1);

        float xDensity = baseDensity * width;
        float yDensity = baseDensity * height;
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
