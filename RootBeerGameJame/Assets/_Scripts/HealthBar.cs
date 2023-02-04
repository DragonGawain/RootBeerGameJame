using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    public Slider slider;
    public Gradient gradient;
    public Image fill;

    private void Start()
    {
        SetHealth(1f);
    }
    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;

        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(float health)
    {
        //slider.value = health;

        //fill.color = gradient.Evaluate(slider.normalizedValue);

        DOTween.To(() => slider.value, x => slider.value = x, health, 2);
    }

    private float UpdateTween()
    {
        throw new NotImplementedException();
    }
}