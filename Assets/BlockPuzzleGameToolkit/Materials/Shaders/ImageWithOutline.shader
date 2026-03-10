Shader "UI/ImageWithOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 2
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.01
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };
            
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _AlphaThreshold;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // 如果当前像素有透明度，检查周围像素
                if (color.a <= _AlphaThreshold)
                {
                    // 检查周围8个方向
                    float2 offsets[8] = {
                        float2(-1, -1), float2(0, -1), float2(1, -1),
                        float2(-1, 0),                 float2(1, 0),
                        float2(-1, 1),  float2(0, 1),  float2(1, 1)
                    };
                    
                    // 检查更大范围的描边
                    for (int i = 0; i < 8; i++)
                    {
                        float2 offset = offsets[i] * _OutlineWidth * _MainTex_TexelSize.xy;
                        half4 neighbor = tex2D(_MainTex, IN.texcoord + offset);
                        if (neighbor.a > _AlphaThreshold)
                        {
                            // 应用Image组件的透明度到描边颜色
                            fixed4 outlineColor = _OutlineColor;
                            outlineColor.a *= IN.color.a;
                            return outlineColor;
                        }
                    }
                    
                    // 检查4个主要方向（更宽的描边）
                    float2 mainOffsets[4] = {
                        float2(-_OutlineWidth, 0) * _MainTex_TexelSize.xy,
                        float2(_OutlineWidth, 0) * _MainTex_TexelSize.xy,
                        float2(0, -_OutlineWidth) * _MainTex_TexelSize.xy,
                        float2(0, _OutlineWidth) * _MainTex_TexelSize.xy
                    };
                    
                    for (int j = 0; j < 4; j++)
                    {
                        half4 neighbor = tex2D(_MainTex, IN.texcoord + mainOffsets[j]);
                        if (neighbor.a > _AlphaThreshold)
                        {
                            // 应用Image组件的透明度到描边颜色
                            fixed4 outlineColor = _OutlineColor;
                            outlineColor.a *= IN.color.a;
                            return outlineColor;
                        }
                    }
                    
                    discard;
                    return fixed4(0, 0, 0, 0);
                }
                
                return color;
            }
            ENDCG
        }
    }
}