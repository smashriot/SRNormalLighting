// -------------------------------------------------------------------------------------------------
//  SRNormalLighting.cs
//  Normal Lit Sprites
//  Created by Jesse Ozog (code@smashriot.com) on 2015/02/23
//  Copyright 2015 SmashRiot, LLC. All rights reserved.
// -------------------------------------------------------------------------------------------------
using UnityEngine;

// ------------------------------------------------------------------------
// SRNormalLighting
// ------------------------------------------------------------------------
public class SRNormalLighting : MonoBehaviour {

    private const string ROCKS_SPRITE = "rocks"; 	
    private const string ROCKS_NORMAL = "rocks_n"; 	
    private GameObject lightGameObject;
    private Light lightSource;
    private float counter = 0;
    private float lightDepth = -20;
    
	// ------------------------------------------------------------------------
	// Use this for initialization
	// ------------------------------------------------------------------------
	public void Start(){

		// init futile
		FutileParams fparms = new FutileParams(true,true,true,true);
		fparms.AddResolutionLevel(1280.0f, 2.0f, 1.0f, "");
		fparms.origin = new Vector2(0.5f, 0.5f);
		Futile.instance.Init(fparms);
		Futile.instance.camera.clearFlags = CameraClearFlags.SolidColor;
		Futile.instance.camera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

		// load main image
		Futile.atlasManager.LoadImage(ROCKS_SPRITE);
		
		// setup image with shader and light
        this.setupLighting();
	}
	
    // ------------------------------------------------------------------------
	// ------------------------------------------------------------------------
	private void setupLighting(){
		
		// define the shader and reuse this shader on the sprites so the FRenderLayers continue to batch properly
        // SRLightingShader(string normalTexture, float shininess, Color diffuseColor, Color specularColor)
		SRLightingShader lightingShader = new SRLightingShader(ROCKS_NORMAL, 2.5f, Color.white, Color.white);
		
		// sprite uses the SRLightingShader for normal mapped lighting
		FSprite leftRockSprite = new FSprite(ROCKS_SPRITE); 
		leftRockSprite.shader = lightingShader; // do NOT create a new Shader for each sprite. Doing so would break FRenderLayer batching
		leftRockSprite.SetAnchor(1.0f, 0.5f);
		Futile.stage.AddChild(leftRockSprite);
		
        // sprite uses the SRLightingShader for normal mapped lighting
		FSprite rightRockSprite = new FSprite(ROCKS_SPRITE); 
		rightRockSprite.shader = lightingShader; // do NOT create a new Shader for each sprite. Doing so would break FRenderLayer batching
		rightRockSprite.SetAnchor(0.0f, 0.5f);
		Futile.stage.AddChild(rightRockSprite);

		// add light gameobject
        lightGameObject = new GameObject("Light");
		lightGameObject.transform.localPosition = new Vector3(0, 0, lightDepth);
		
  		// add lightsource to it and configure
  		lightSource = lightGameObject.AddComponent<Light>();
        lightSource.color = Color.white;
        lightSource.intensity = 8;
        lightSource.range = 375;
        lightSource.type = LightType.Point;
        lightSource.renderMode = LightRenderMode.ForcePixel; // ForcePixel = Important
    }
    

	// ------------------------------------------------------------------------
	// ------------------------------------------------------------------------
	public void Update(){
        
        counter += 0.5f * Time.deltaTime;
        
        // circle light around and up/down between -30..0. light must be negative.
        // closer to 0 = tight spot, farther away = wider spot.
        lightGameObject.transform.localPosition = new Vector3(150.0f * Mathf.Cos(counter), 150.0f * Mathf.Sin(counter), -Mathf.Abs(lightDepth * Mathf.Sin(0.5f * counter)));
        lightSource.color = this.HSVtoRGB(Mathf.Abs(Mathf.Sin(0.5f * counter)), 1.0f, 1.0f, 1.0f);
	}
	
    // ------------------------------------------------------------------------
    // using hsv to rgb to easily rotate light color by hue
    // ------------------------------------------------------------------------
    public Color HSVtoRGB(float hue, float saturation, float value, float alpha){

        while (hue > 1.0f) { hue -= 1.0f; }
        while (hue < 0.0f) { hue += 1.0f; }
        while (saturation > 1.0f) { saturation -= 1.0f; }
        while (saturation < 0.0f) { saturation += 1.0f; }
        while (value > 1.0f) { value -= 1.0f; }
        while (value < 0.0f) { value += 1.0f; }
        if (hue > 0.999f) { hue = 0.999f; }
        if (hue < 0.001f) { hue = 0.001f; }
        if (saturation > 0.999f) { saturation = 0.999f; }
        if (saturation < 0.001f) { return new Color(value * 255.0f, value * 255.0f, value * 255.0f); }
        if (value > 0.999f) { value = 0.999f; }
        if (value < 0.001f) { value = 0.001f; }

        float h6 = hue * 6.0f;
        if (h6 == 6.0f) { h6 = 0.0f; }
        int ihue = (int)(h6);
        float p = value * (1.0f - saturation);
        float q = value * (1.0f - (saturation * (h6 - (float)ihue)));
        float t = value * (1.0f - (saturation * (1.0f - (h6 - (float)ihue))));
        switch (ihue){
            case 0:  return new Color(value, t, p, alpha);
            case 1:  return new Color(q, value, p, alpha);
            case 2:  return new Color(p, value, t, alpha);
            case 3:  return new Color(p, q, value, alpha);
            case 4:  return new Color(t, p, value, alpha);
            default: return new Color(value, p, q, alpha);
        }
    }

}