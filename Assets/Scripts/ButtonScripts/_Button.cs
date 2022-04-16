using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public abstract class Button : MonoBehaviour {

	public KMSelectable btn;
	public MeshRenderer flashingRenderer;
	private KMSelectable parentMod;
	public Dir direction { get; set; }

	protected const RegexOptions regexFlags = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
	public abstract Predicate<string> rule { get; }
	public abstract string name { get; }

	void Start () {
		parentMod = GetComponentInParent<ModuleManeuversScript>().GetComponent<KMSelectable>();
	}

	void SetChildStatus(bool enable)
    {
		if (enable)
        {
            for (int i = 0; i < parentMod.Children.Length; i++)
            {
				if (parentMod.Children[i] == null)
                {
					parentMod.Children[i] = btn;
					break;
                }
            }
        }
        else
			parentMod.Children[Array.IndexOf(parentMod.Children, btn)] = null;
    }

	IEnumerator FlashButton()
    {
		Color initC = flashingRenderer.material.color;
		flashingRenderer.material.color = Color.white;
		yield return new WaitForSeconds(0.75f);
		flashingRenderer.material.color = initC;
		yield return new WaitForSeconds(0.75f);
    }
}
