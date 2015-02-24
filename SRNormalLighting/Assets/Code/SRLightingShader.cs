// -------------------------------------------------------------------------------------------------
//  SRLightingShader.cs
//  Normal Lit Sprites
//  Created by Jesse Ozog (code@smashriot.com) on 2015/02/23
//  Copyright 2015 SmashRiot, LLC. All rights reserved.
//
// NOTE: uses 0.92.0 Futile (unstable - https://github.com/MattRix/Futile/tree/unstable)
//  - The shader interface is different in 0.91.0 Master/Dev.
// -------------------------------------------------------------------------------------------------
using UnityEngine;

// ------------------------------------------------------------------------
// Supports normal mapped lighting
// ------------------------------------------------------------------------
public class SRLightingShader : FShader {

	private string _normalTexture;
	private float _shininess;
	private Color _diffuseColor;
	private Color _specularColor;
	
	// ------------------------------------------------------------------------
	// normalTexture = full path/name to normal map for corresponding main texture for this mat: e.g. Images/tiles_n
	// ------------------------------------------------------------------------
	public SRLightingShader(string normalTexture, float shininess, Color diffuseColor, Color specularColor) : 
                       base("SRLighting", Shader.Find("Futile/SRLighting")){

        // assign parms
		_normalTexture = normalTexture;
		_shininess = shininess;
		_diffuseColor = diffuseColor;
		_specularColor = specularColor;

        // ensure Apply gets called
		needsApply = true;
	}

	// ------------------------------------------------------------------------
    // applies these parameters to the material for the shader
	// ------------------------------------------------------------------------
	override public void Apply(Material mat){

		// load normal texture for this shader
		Texture2D normalTex = Resources.Load(_normalTexture) as Texture2D;		
		mat.SetTexture("_NormalTex", normalTex);
		mat.SetFloat("_Shininess", _shininess);
		mat.SetColor("_Color", _diffuseColor); // diffuse
		mat.SetColor("_SpecColor", _specularColor);
	}
}
