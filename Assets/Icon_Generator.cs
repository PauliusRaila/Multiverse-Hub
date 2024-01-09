using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Icon_Generator : MonoBehaviour
{
    public void generateObjectImage(Transform obj , RawImage img) {

        RuntimePreviewGenerator.BackgroundColor = new Color(0, 0, 0, 0);
        RuntimePreviewGenerator.OrthographicMode = true;
        img.texture = RuntimePreviewGenerator.GenerateModelPreview(obj, 128, 128);

    }
}
