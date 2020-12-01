﻿using PhosphorescenceExtensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;

internal static class Function
{
    internal static void PlaySound(string sound, PhosphorescenceScript pho)
    {
        pho.Audio.PlaySoundAtTransform(sound, pho.transform);
    }

    internal static float ElasticIn(float k)
    {
        return k % 1 == 0 ? k : -Mathf.Pow(2f, 10f * (k -= 1f)) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f);
    }

    internal static float ElasticOut(float k)
    {
        return k % 1 == 0 ? k : Mathf.Pow(2f, -10f * k) * Mathf.Sin((k - 0.1f) * (2f * Mathf.PI) / 0.4f) + 1f;
    }

    internal static void OnInteractArray(KMSelectable[] selectables, Func<int, KMSelectable.OnInteractHandler> method)
    {
        for (int i = 0; i < selectables.Length; i++)
        {
            int j = i;
            selectables[i].OnInteract += method(j);
        }
    }

    internal static bool[] RandomBools(int length)
    {
        bool[] array = new bool[length];
        for (int i = 0; i < array.Length; i++)
            array[i] = Rnd.Range(0, 1f) > 0.5;
        return array;
    }

    internal static void SetIntertwinedColor(Renderer renderer, Color32 colorA, Color32 colorB, float f)
    {
        float negF = 1 - f;
        renderer.material.color = new Color32((byte)((colorA.r * negF) + (colorB.r * f)), (byte)((colorA.g * negF) + (colorB.g * f)), (byte)((colorA.b * negF) + (colorB.b * f)), 255);
    }

    internal static string[] GetAllAnswers(string solution, int index)
    {
        List<string>[] answers = new List<string>[solution.Length];

        for (int i = 0; i < answers.Length; i++)
            answers[i] = new List<string>();

        for (int i = 0; i < solution.Length; i++)
        {
            foreach (string button in Enum.GetNames(typeof(ButtonType)))
            {
                if (solution[i] == button[(index + i) % button.Length].ToString().ToLowerInvariant().ToCharArray()[0])
                {
                    string nextAnswer = button == "Black" ? "K" : button[0].ToString();

                    if (i == 0)
                        answers[i].Add(nextAnswer);
                    else
                        foreach (string answer in answers[i - 1])
                            answers[i].Add(answer + nextAnswer);
                }
            }
        }

        return answers[answers.Length - 1].ToArray();
    }

    internal static Color GetColor(ButtonType buttonType)
    {
        switch (buttonType)
        {
            case ButtonType.Black: return Color.black;
            case ButtonType.Red: return Color.red;
            case ButtonType.Green: return Color.green;
            case ButtonType.Blue: return Color.blue;
            case ButtonType.Cyan: return Color.cyan;
            case ButtonType.Magenta: return Color.magenta;
            case ButtonType.Yellow: return Color.yellow;
            case ButtonType.White: return Color.white;
            default: throw new NotImplementedException();
        }
    }

    internal static void CountSound(int i, PhosphorescenceScript pho, bool exception)
    {
        if (exception)
            return;

        PlaySound("timerTick", pho);

        switch (i)
        {
            case 1: PlaySound("voice_one", pho); break;
            case 2: PlaySound("voice_two", pho); break;
            case 3: PlaySound("voice_three", pho); break;
            case 4: PlaySound("voice_four", pho); break;
            case 5: PlaySound("voice_five", pho); break;
            case 6: PlaySound("voice_six", pho); break;
            case 7: PlaySound("voice_seven", pho); break;
            case 8: PlaySound("voice_eight", pho); break;
            case 9: PlaySound("voice_nine", pho); break;
            case 30: PlaySound("voice_thirtyseconds", pho); break;
            case 60: PlaySound("voice_oneminute", pho); break;
            case 120: PlaySound("voice_twominutes", pho); break;
            case 180: PlaySound("voice_threeminutes", pho); break;
            case 240: PlaySound("voice_fourminutes", pho); break;
            default: if (i % 60 == 0) PlaySound("notableTimeLeft", pho); break;
        }
    }

    internal static int GetLCount(bool[] colors)
    {
        const int s = 7;
        int count = 0;

        for (int i = 0; i < s - 1; i++)
            for (int j = 0; j < s - 1; j++)
            {
                if (colors[(i * s) + j] && colors[(i * s) + j + 1] && colors[((i + 1) * s) + j] && !colors[((i + 1) * s) + j + 1])
                    count++;
                if (colors[(i * s) + j] && colors[(i * s) + j + 1] && !colors[((i + 1) * s) + j] && colors[((i + 1) * s) + j + 1])
                    count++;
                if (colors[(i * s) + j] && !colors[(i * s) + j + 1] && colors[((i + 1) * s) + j] && colors[((i + 1) * s) + j + 1])
                    count++;
                if (!colors[(i * s) + j] && colors[(i * s) + j + 1] && colors[((i + 1) * s) + j] && colors[((i + 1) * s) + j + 1])
                    count++;

                if (!colors[(i * s) + j] && !colors[(i * s) + j + 1] && !colors[((i + 1) * s) + j] && colors[((i + 1) * s) + j + 1])
                    count++;
                if (!colors[(i * s) + j] && !colors[(i * s) + j + 1] && colors[((i + 1) * s) + j] && !colors[((i + 1) * s) + j + 1])
                    count++;
                if (!colors[(i * s) + j] && colors[(i * s) + j + 1] && !colors[((i + 1) * s) + j] && !colors[((i + 1) * s) + j + 1])
                    count++;
                if (colors[(i * s) + j] && !colors[(i * s) + j + 1] && !colors[((i + 1) * s) + j] && !colors[((i + 1) * s) + j + 1])
                    count++;
            }

        return count;
    }
}