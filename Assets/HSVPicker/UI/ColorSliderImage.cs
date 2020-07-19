﻿using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage)), ExecuteInEditMode]
public class ColorSliderImage : MonoBehaviour
{
    public ColorPicker picker;

    /// <summary>
    /// Which value this slider can edit.
    /// </summary>
    public ColorValues type;

    public Slider.Direction direction;

    private RawImage image;

    private void Awake()
    {
        image = GetComponent<RawImage>();
        if(Application.isPlaying)
            RegenerateTexture();
    }

    private void OnEnable()
    {
        if(picker == null || !Application.isPlaying)
            return;
        picker.onValueChanged.AddListener(ColorChanged);
        picker.onHSVChanged.AddListener(HSVChanged);
    }

    private void OnDisable()
    {
        if(picker == null)
            return;
        picker.onValueChanged.RemoveListener(ColorChanged);
        picker.onHSVChanged.RemoveListener(HSVChanged);
    }

    private void OnDestroy()
    {
        if(image.texture != null)
            DestroyImmediate(image.texture);
    }

    private void ColorChanged(Color newColor)
    {
        if(type == ColorValues.R
           || type == ColorValues.G
           || type == ColorValues.B
           || type == ColorValues.Saturation
           || type == ColorValues.Value)
            RegenerateTexture();
    }

    private void HSVChanged(float hue, float saturation, float value)
    {
        if(type == ColorValues.R
           || type == ColorValues.G
           || type == ColorValues.B
           || type == ColorValues.Saturation
           || type == ColorValues.Value)
            RegenerateTexture();
    }

    private void RegenerateTexture()
    {
        Color32 baseColor = picker != null ? picker.CurrentColor : Color.black;
        var h = picker != null ? picker.H : 0;
        var s = picker != null ? picker.S : 0;
        var v = picker != null ? picker.V : 0;
        var vertical = direction == Slider.Direction.BottomToTop || direction == Slider.Direction.TopToBottom;
        var inverted = direction == Slider.Direction.TopToBottom || direction == Slider.Direction.RightToLeft;
        int size;
        switch(type)
        {
            case ColorValues.R:
            case ColorValues.G:
            case ColorValues.B:
            case ColorValues.A:
                size = 255;
                break;
            case ColorValues.Hue:
                size = 360;
                break;
            case ColorValues.Saturation:
            case ColorValues.Value:
                size = 100;
                break;
            default:
                throw new NotImplementedException("");
        }
        var texture = vertical
            ? new Texture2D(1, size)
            : new Texture2D(size, 1);
        texture.hideFlags = HideFlags.DontSave;
        var colors = new Color32[size];
        switch(type)
        {
            case ColorValues.R:
                for(byte i = 0; i < size; i++)
                    colors[inverted ? size - 1 - i : i] = new Color32(i, baseColor.g, baseColor.b, 255);
                break;
            case ColorValues.G:
                for(byte i = 0; i < size; i++)
                    colors[inverted ? size - 1 - i : i] = new Color32(baseColor.r, i, baseColor.b, 255);
                break;
            case ColorValues.B:
                for(byte i = 0; i < size; i++)
                    colors[inverted ? size - 1 - i : i] = new Color32(baseColor.r, baseColor.g, i, 255);
                break;
            case ColorValues.A:
                for(byte i = 0; i < size; i++)
                    colors[inverted ? size - 1 - i : i] = new Color32(i, i, i, 255);
                break;
            case ColorValues.Hue:
                for(var i = 0; i < size; i++)
                    colors[inverted ? size - 1 - i : i] = HSVUtil.ConvertHsvToRgb(i, 1, 1, 1);
                break;
            case ColorValues.Saturation:
                for(var i = 0; i < size; i++)
                    colors[inverted ? size - 1 - i : i] = HSVUtil.ConvertHsvToRgb(h * 360, (float)i / size, v, 1);
                break;
            case ColorValues.Value:
                for(var i = 0; i < size; i++)
                    colors[inverted ? size - 1 - i : i] = HSVUtil.ConvertHsvToRgb(h * 360, s, (float)i / size, 1);
                break;
            default:
                throw new NotImplementedException("");
        }
        texture.SetPixels32(colors);
        texture.Apply();
        if(image.texture != null)
            DestroyImmediate(image.texture);
        image.texture = texture;
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch(direction)
        {
            case Slider.Direction.BottomToTop:
            case Slider.Direction.TopToBottom:
                image.uvRect = new Rect(0, 0, 2, 1);
                break;
            case Slider.Direction.LeftToRight:
            case Slider.Direction.RightToLeft:
                image.uvRect = new Rect(0, 0, 1, 2);
                break;
        }
    }
}
