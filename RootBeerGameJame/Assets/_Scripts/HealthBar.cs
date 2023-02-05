using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;

    private DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> tween;

    public float _health;

    private Animator animator;

    private void Start()
    {
        _health = 0.0f;

        slider.value = _health;

        animator = GetComponent<Animator>();
    }

    public void AddHealth(float healthDelta)
    {
        if (tween != null)
        {
            tween.Kill();
        }

        _health = Mathf.Min(1.25f, _health + healthDelta);

        tween = DOTween.To(() => slider.value, x => slider.value = x, _health, 0.1f);

        animator.SetBool("Shake", _health >= 1.0f);
    }

    public bool LoseHealth(float healthDelta)
    {
        if (tween != null)
        {
            tween.Kill();
        }

        _health = Mathf.Min(_health, 1.0f);

        _health = Mathf.Max(0.0f, _health - healthDelta);

        slider.value = _health;

        animator.SetBool("Shake", _health >= 1.0f);

        return _health != 0.0f;
    }

    public bool Alive()
    {
        return _health > 0.0f;
    }

    public bool IsDead()
    {
        return _health == 1.25f;
    }
}