// -------------------------------------------------------------------------------------------------
//  SRLighting.shader
//  Normal Lit Sprites
//  Created by Jesse Ozog (code@smashriot.com) on 2015/02/23
//  Copyright 2015 SmashRiot, LLC. All rights reserved.
//
// NOTES: 
//  Lighting needs Forward Rendering: Player Settings -> Rendering Path = Forward
//  Uses UNITY_LIGHTMODEL_AMBIENT, which is set under Edit -> Render Settings -> Ambient Light 
//
// LOTS OF REFERENCES:
//  https://www.youtube.com/watch?v=bqKULvitmpU (P5 N*L)
//  http://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
//  http://docs.unity3d.com/Manual/SL-BuiltinValues.html
//  http://docs.unity3d.com/Manual/SL-BuiltinIncludes.html
//  http://docs.unity3d.com/Manual/SL-PassTags.html
//  http://www.alkemi-games.com/a-game-of-tricks/
//  http://indreams-studios.com/post/writing-a-spritelamp-shader-in-unity/
//  http://indreams-studios.com/SpriteLamp.shader
// -------------------------------------------------------------------------------------------------
Shader "Futile/SRLighting" { 

    Properties {
        _MainTex ("Base RGBA", 2D) = "white" {}
        _NormalTex ("Normalmap", 2D) = "bump" {}
        _Color ("Diffuse Material Color", Color) = (1.0, 1.0, 1.0, 1.0) 
        _SpecularColor ("Specular Material Color", Color) = (1.0, 1.0, 1.0, 1.0) 
        _Shininess ("Shininess", Float) = 5
    }
    
    SubShader {
        // these are applied to all of the Passes in this SubShader
        ZWrite Off
		ZTest Always
		Fog { Mode Off }
		Lighting On
    	Cull Off
    	
// -------------------------------------
// Base pass: Sets frag to text+ambient light+color
// -------------------------------------
        Pass {    

            Tags { "LightMode" = "ForwardBase" "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" } 
            Blend SrcAlpha OneMinusSrcAlpha 
		
CGPROGRAM

#pragma vertex vert  
#pragma fragment frag 

#include "UnityCG.cginc"

uniform sampler2D _MainTex;

struct VertexInput {

    float4 vertex : SV_POSITION; // used instead of POSITION;
    float4 color : COLOR;
    float4 uv : TEXCOORD0;    
};

struct VertexOutput {

    float4 pos : SV_POSITION; // used instead of POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
};

VertexOutput vert(VertexInput i){

    VertexOutput o;

    o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
    o.color = i.color; 
    o.uv = float2(i.uv);
    
    return o;
}

float4 frag(VertexOutput i) : COLOR {

    float4 diffuseColor = tex2D(_MainTex, i.uv);
    float3 ambientLighting = float3(UNITY_LIGHTMODEL_AMBIENT) * float3(diffuseColor) * float3(i.color);
    
    return float4(ambientLighting, diffuseColor.a);
}

ENDCG
        }
        
// -------------------------------------
// Lighting Pass: Lights must be set to Important
// -------------------------------------
        Pass {	

            Tags { "LightMode" = "ForwardAdd" "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
            Blend One One // additive blending 

CGPROGRAM

#pragma vertex vert  
#pragma fragment frag 

#include "UnityCG.cginc"

// shader uniforms
uniform sampler2D _MainTex;   // source diffuse texture
uniform sampler2D _NormalTex; // normal map lighting texture (set to import type: Lightmap)
uniform float4 _LightColor0;  // color of light source 
uniform float4 _SpecularColor; 
uniform float _Shininess;
uniform float4 _ObjectSpaceLightPos;
            
struct vertexInput {
    float4 vertex : SV_POSITION; // used instead of POSITION 
    float4 color : COLOR;
    float4 uv : TEXCOORD0;  
};

struct fragmentInput {
    float4 pos : SV_POSITION; // used instead of POSITION
    float4 color : COLOR0;
    float2 uv : TEXCOORD0;
    float4 posWorld : TEXCOORD1; 

};

// -------------------------------------
fragmentInput vert(vertexInput i){

    fragmentInput o;
    
    o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
    o.posWorld = mul(_Object2World, i.vertex);
    
    o.uv = float2(i.uv);
    o.color = i.color;
                
    return o;
}

// -------------------------------------
float4 frag(fragmentInput i) : COLOR {
            
    // dist to point light
    float3 vertexToLightSource = float3(_WorldSpaceLightPos0) - i.posWorld;
    float3 distance = length(vertexToLightSource);    

    // calc attenuation
    float attenuation = 1.0 / distance; 
    float3 lightDirection = normalize(vertexToLightSource);
            
    // get value from normal map and sub 0.5 and mul by 2 to change RGB range 0..1 to normal range -1..1
    float3 normalDirection = (tex2D(_NormalTex, i.uv).xyz - 0.5f) * 2.0f;
    
    // mul by world to object matrix, which handles rotation, etc
    normalDirection = float3(mul(float4(normalDirection, 0.5f), _World2Object));
    
    // negate Z so that lighting works as expected (sprites further away from the camera than a light are lit, etc.)
    normalDirection.z *= -1;
    
    // normalize direction
    normalDirection = normalize(normalDirection); 

    // calc diffuse lighting
    float normalDotLight = dot(normalDirection, lightDirection);
    float diffuseLevel = attenuation * normalDotLight; // removed max(0.0, normalDotLight); since assumiing normalDotLight > 0.0 
    
    // calc specular ligthing
    // assuming normalDotLight > 0.0 (meaning light is on the correct side of mesh) as an optimization to avoid branching.
    float3 viewDirection = float3(0.0, 0.0, -1.0); //  orthographic
    float specularLevel = attenuation * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);

    // calc color components
    float4 diffuseColor = tex2D(_MainTex, i.uv);
    float3 diffuseReflection = float3(diffuseColor) * diffuseLevel * i.color * float3(_LightColor0);
    float3 specularReflection = float3(_SpecularColor) * specularLevel * i.color * float3(_LightColor0);
    
    // use the alpha from diffuse. it's not perfect if not also mul by diffuseColor.a 
    return diffuseColor.a * float4(diffuseReflection + specularReflection, diffuseColor.a);
}    

ENDCG        
        } // end Pass
// -------------------------------------
// -------------------------------------
 
   } // end SubShader
   
   // fallback shader - comment out during dev
   // Fallback "Diffuse"
}