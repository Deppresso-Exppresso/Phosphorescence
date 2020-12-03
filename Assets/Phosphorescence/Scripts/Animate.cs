﻿using PhosphorescenceExtensions;
using System;
using System.Collections;
using UnityEngine;
using Rnd = UnityEngine.Random;

/// <summary>
/// This class handles any "pure" animations.
/// Every method in here is an IEnumerator which is meant to be called by Phosporescence itself.
/// </summary>
internal class Animate
{
    internal Animate(PhosphorescenceScript pho, Init init, Select select, Render render)
    {
        _pho = pho;
        _init = init;
        _select = select;
        _render = render;
    }

    private readonly PhosphorescenceScript _pho;
    private readonly Init _init;
    private readonly Select _select;
    private readonly Render _render;

    /// <summary>
    /// Used to cancel animations already running relating to button presses, such that IEnumerators don't clash with each other.
    /// </summary>
    private bool isPushingButton;

    /// <summary>
    /// Transitions the module to an active state, by first playing an animation.
    /// </summary>
    internal IEnumerator Run()
    {
        _init.isAnimated = true;
        Function.PlaySound("start", _pho);

        // This makes the display darker, since it always returns 0 in binary.
        _init.index = 0; 

        float solved = _pho.Info.GetSolvedModuleNames().Count,
              solvable = _pho.Info.GetSolvableModuleNames().Count,
              deltaSolved = solved + 1 == solvable ? 1 : solved / solvable;

        // The more modules are solved, the more time given.
        int currentTime = Init.streamDelay + 300 + (int)(deltaSolved * 300); 

        // Because of user-inputted mod settings, we need to make sure it doesn't go outside the confines of the 7-segment display.
        Render.currentTime = Mathf.Max(Mathf.Min(currentTime, 5999), 10);

        // This loop runs an animation of the timer increasing rapidly.
        for (int i = 0; i < Render.currentTime; i += (int)Mathf.Ceil((float)Render.currentTime / 100))
        {
            _render.UpdateDisplay(i);
            yield return new WaitForSecondsRealtime(0.02f);
        }

        // Most of the time this will only need to run once. This is a failsafe to make sure that there are at least 3 answers.
        do _init.index = Rnd.Range(0, Words.ValidWords.GetLength(0));
        while (Words.ValidWords[_init.index].Length < 3);

        // Pick any solution from the current index.
        _init.solution = Words.ValidWords[_init.index].PickRandom();

        // Log the current answer.
        Debug.LogFormat("[Phosphorescence #{0}]: The expected answer is {1}, deriving from the starting offset {2}.", _init.moduleId, _init.solution, _init.index);
        Debug.LogFormat("[Phosphorescence #{0}]: All possible answers are: {1}.", _init.moduleId, Function.GetAllAnswers(_init.solution, _init.index).Join(", "));

        _pho.StartCoroutine(_render.Countdown());

        _init.isCountingDown = true;
        _init.isAnimated = false;
    }

    /// <summary>
    /// Animation for pressing down the button.
    /// </summary>
    /// <param name="transform">The transform of the button.</param>
    internal IEnumerator PressButton(Transform transform)
    {
        // While messy, this ensures that any button already pushed will quit their animation.
        _init.isAnimated = true;
        isPushingButton = true;
        yield return new WaitForSecondsRealtime(0.1f);
        isPushingButton = false;
        _init.isAnimated = false;

        // ElasticIn ease of the button being pushed down.
        float k = 1;
        while (k > 0 && !isPushingButton)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, -2 * Function.ElasticIn(k), transform.localPosition.z);
            k -= 0.0078125f;
            yield return new WaitForSecondsRealtime(0.01f);
        }

        // Resets the button's position.
        transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
    }

    /// <summary>
    /// Flashes all buttons white, fading them into different colors.
    /// </summary>
    internal IEnumerator ResetButtons()
    {
        // Similarily to PressButton, this ensures that any btuton already pushed will quit their animation.
        _init.isAnimated = true;
        isPushingButton = true;
        yield return new WaitForSecondsRealtime(0.1f);
        isPushingButton = false;
        _init.isAnimated = false;

        // Clear current submission.
        _init.submission = string.Empty;
        _init.buttonPresses = new ButtonType[0];

        _select.ShuffleButtons();
        Function.PlaySound("shuffleButtons", _pho);
        
        // ElasticÍn ease of all buttons being pushed down.
        float k = 1;
        while (k > 0 && !isPushingButton)
        {
            for (int i = 0; i < _pho.ButtonRenderers.Length; i++)
            {
                _pho.ButtonRenderers[i].transform.localPosition = new Vector3(_pho.ButtonRenderers[i].transform.localPosition.x, -2 * Function.ElasticIn(k), _pho.ButtonRenderers[i].transform.localPosition.z);
                Function.SetIntertwinedColor(renderer: _pho.ButtonRenderers[i],
                                             colorA: Function.GetColor(_select.buttons[i]),
                                             colorB: Color.white,
                                             f: Math.Max((k - 0.75f) * 4, 0));
            }

            k -= 0.0078125f;
            yield return new WaitForSecondsRealtime(0.01f);
        }

        // Resets all buttons' positions.
        for (int i = 0; i < _pho.ButtonRenderers.Length; i++)
        {
            _pho.ButtonRenderers[i].transform.localPosition = new Vector3(_pho.ButtonRenderers[i].transform.localPosition.x, 0, _pho.ButtonRenderers[i].transform.localPosition.z);
            _pho.ButtonRenderers[i].material.color = Function.GetColor(_select.buttons[i]);
        }

        yield return FadeButtons();
    }
    
    /// <summary>
    /// This method should get called to fade the buttons to black. Any button push will cut the animation short.
    /// </summary>
    internal IEnumerator FadeButtons()
    {
        yield return new WaitForSecondsRealtime(0.02f);

        // Gradually turns the buttons black.
        float k = 0;
        while (k <= 1 && !isPushingButton)
        {
            for (int i = 0; i < _pho.ButtonRenderers.Length; i++)
                Function.SetIntertwinedColor(renderer: _pho.ButtonRenderers[i],
                                             colorA: Function.GetColor(_select.buttons[i]),
                                             colorB: Color.black,
                                             f: k);

            k += 0.001953125f;
            yield return new WaitForSecondsRealtime(0.01f);
        }

        // Sets them to black immeditely.
        for (int i = 0; i < _pho.ButtonRenderers.Length; i++)
            _pho.ButtonRenderers[i].material.color = Color.black;
    }

    /// <summary>
    /// Covers the transition between an active state to an active submission state.
    /// </summary>
    internal IEnumerator EnterSubmit()
    {
        _init.isAnimated = true;
        _pho.StartCoroutine(FadeButtons());

        // ElasticOut ease of screen going down.
        float k = 1;
        while (k > 0)
        {
            _pho.Screen.transform.localPosition = new Vector3(-0.015f, (0.02f * Function.ElasticOut(k)) - 0.015f, -0.016f);
            k -= 0.015625f;
            yield return new WaitForSecondsRealtime(0.01f);
        }

        // ElasticOut ease of buttons going up.
        k = 0;
        while (k <= 1)
        {
            _pho.Panel.transform.localPosition = new Vector3(0, (0.035f * Function.ElasticOut(k)) - 0.025f, 0);
            k += 0.00390625f;
            yield return new WaitForSecondsRealtime(0.01f);
        }

        _init.isAnimated = false;
    }

    /// <summary>
    /// Covers the transition between an active submission state to a validation state.
    /// </summary>
    internal IEnumerator ExitSubmit()
    {
        _init.isAnimated = true;
        _init.isCountingDown = false;

        Function.PlaySound("endSubmit", _pho);

        // ElasticOut ease of buttons going down.
        float k = 0;
        while (k <= 1)
        {
            _pho.Panel.transform.localPosition = new Vector3(0, (0.035f * Function.ElasticOut(1 - k)) - 0.025f, 0);
            k += 0.00390625f;
            yield return new WaitForSecondsRealtime(0.01f);
        }

        // ElasticOut ease of screen going up.
        k = 1;
        while (k > 0)
        {
            _pho.Screen.transform.localPosition = new Vector3(-0.015f, (0.02f * Function.ElasticOut(1 - k)) - 0.015f, -0.016f);
            k -= 0.015625f;
            yield return new WaitForSecondsRealtime(0.01f);
        }

        // Validation check.
        if (_init.solution == _init.submission)
            _pho.StartCoroutine(_init.Solve());
        else
            _pho.StartCoroutine(_init.BufferStrike());

        // Make the module inactive.
        _init.isAnimated = false;
        _init.isInSubmission = false;
        _init.isSelected = false;
    }

    /// <summary>
    /// An animation used after the module is solved. This runs indefinitely.
    /// </summary>
    /// <param name="displayStates">An array that matches with pho's display renderer array.</param>
    internal IEnumerator PostSolve(PhosphorescenceScript pho, int[] displayStates)
    {
        while (true)
        {
            // For each corner.
            for (int i = 0; i < 4; i++)
                // 20 is an arbitrary number that works well here.
                for (int j = 0; j < 20; j++)
                {
                    // For each display.
                    for (int k = 0; k < displayStates.Length; k++)
                    {
                        // Should the screen change color right now?
                        if ((i % 4 == 0 && (k % 7) + (k / 7) == j) || // Top-left
                            (i % 4 == 3 && (k % 7) + (7 - (k / 7)) == j) || // Bottom-left
                            (i % 4 == 1 && 7 - (k % 7) + (k / 7) == j) || // Top-right
                            (i % 4 == 2 && 7 - (k % 7) + (7 - (k / 7)) == j)) // Bottom-right
                            displayStates[k] = ++displayStates[k] % 8;
                        pho.Tiles[k].material.color = Function.GetColor((ButtonType)displayStates[k]);
                    }

                    yield return new WaitForSecondsRealtime(0.2f);
                }
        }
    }
}
