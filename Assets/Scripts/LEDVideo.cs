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
    [FormerlySerializedAs("baseSize")] [SerializeField] private FloatPair baseVideoSize;
    [FormerlySerializedAs("baseSize")] [SerializeField] private FloatPair baseImageSize;

    private float baseDensity;
    
    [Header("Video Clip")]
    [SerializeField] private List<VideoClip> videoClips = new List<VideoClip>();
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RenderTexture renderTexture;
    
    [Header("Images")]
    [SerializeField] private List<Texture> images = new List<Texture>();
    
    private int currentClipIndex = 0;
    private int currentImageIndex = 0;

    private void Start()
    {
        this.transform.localScale = new Vector3(baseVideoSize.x, baseVideoSize.y, 1);
        Material mat  = renderer.material;
        mat.SetFloat("TilingLEDX", baseDensity * baseVideoSize.x);
        mat.SetFloat("TilingLEDY", baseDensity * baseVideoSize.y);
    }

    public void SetVideoClip(int index)
    {
        currentImageIndex = -1; //bad workaround for image to video
        
        if (index == currentClipIndex)
            return;
        
        if(index >= videoClips.Count)
            return;
        
        //change size
        ChangeScreenSize(SceneManager.Instance.GetZoomInt());
        
        //set video clip
        currentClipIndex = index;
        videoPlayer.clip = videoClips[index];
        Material mat  = renderer.material;
        mat.SetTexture("_BaseMape", renderTexture);
        
        //play
        videoPlayer.Play();
        videoPlayer.isLooping = true;
    }

    public void SetImage(int index)
    {
        currentClipIndex = -1; //bad workaround for video to image

        if (index == currentImageIndex)
            return;

        if (index >= images.Count)
            return;
        
        ChangeScreenSize(SceneManager.Instance.GetZoomInt(), false);
        
        //set image
        currentImageIndex = index;
        Material mat  = renderer.material;
        mat.SetTexture("_BaseMape", images[index]);
    }

    public void ChangeScreenSize(int sizeFactor, bool isVideo = true)
    {
        float multiplier = Mathf.Pow(2f, sizeFactor);
        float width, height;
        if (isVideo)
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
