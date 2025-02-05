Shader "Unlit/AgentColour2D"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Size ("Size", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha // Enable transparency

            CGPROGRAM
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _Color[1023];  // Array to handle multiple colors per instance
            float _Size;

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                // Scale the object by the size factor
                v.vertex.xyz *= _Size;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i, uint instanceID : SV_InstanceID) : SV_Target
            {
                return _Color[instanceID];  // Use color based on instanceID
            }
            ENDCG
        }
    }
}