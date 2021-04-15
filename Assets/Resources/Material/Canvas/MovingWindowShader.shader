Shader "MovingWindowShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Blur("Blur Kernel Size", Float) = 10
        _StandardDeviation("Standard Deviation", Float) = 0.4
        _WindowPositionX("Window X Position", Float) = 1.0
        _WindowPositionY("Window Y Position", Float) = 1.0
        _WindowSize("Window Size", Float) = 100
    }

    SubShader
    {

        Tags{ "Queue" = "Transparent" }

        GrabPass
        {   
        }

        // Horizontal Blur
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 vertColor : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.vertColor = v.color;
                return o;
            }

            sampler2D _GrabTexture;
            fixed4 _GrabTexture_TexelSize;
            float _StandardDeviation;
            float _Blur;
            float _WindowPositionX;
            float _WindowPositionY;
            float _WindowSize;

            half4 frag(v2f i) : SV_Target
            {
                float windowXPos = _WindowPositionX;
                float windowYPos = _WindowPositionY;
                float screenRatio = _ScreenParams.y/_ScreenParams.x;
                float opacity = 1;
                float L = sqrt((i.grabPos.x - windowXPos)*(i.grabPos.x - windowXPos) + (i.grabPos.y - (1-windowYPos))*screenRatio*(i.grabPos.y - (1-windowYPos))*screenRatio);
            
                float blur = _Blur;
                blur = max(1, blur);

                fixed4 col = fixed4(0,0,0,0);
                fixed4 windowCol = tex2Dproj(_GrabTexture, i.grabPos);
                float weight_total = 0;

                [loop]
                for (float x = -blur; x <= blur; x += 1)
                {
                    float weight = (1 / (_StandardDeviation * sqrt(2 * 3.14159265359)) * exp(-pow(x, 2)/(2 * pow(_StandardDeviation, 2))));
                    weight_total += weight;
                    col += tex2Dproj(_GrabTexture, i.grabPos + float4(x * _GrabTexture_TexelSize.x, 0, 0, 0)) * weight;
                }
                col /= weight_total;

                if (L < _WindowSize)
                    opacity = 0;
                else
                    opacity = 1;
                
                col = windowCol * (1 - opacity) + col * (opacity);

                return col;
            }
            ENDCG
        }
        GrabPass
        {   
        }

        // Vertical Blur
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 vertColor : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.vertColor = v.color;
                return o;
            }

            sampler2D _GrabTexture;
            fixed4 _GrabTexture_TexelSize;
            float _StandardDeviation;
            float _Blur;
            float _WindowPositionX;
            float _WindowPositionY;
            float _WindowSize;

            half4 frag(v2f i) : SV_Target
            {
                float windowXPos = _WindowPositionX;
                float windowYPos = _WindowPositionY;
                float screenRatio = _ScreenParams.y/_ScreenParams.x;
                float opacity = 1;
                float L = sqrt((i.grabPos.x - windowXPos)*(i.grabPos.x - windowXPos) + (i.grabPos.y - (1-windowYPos))*screenRatio*(i.grabPos.y - (1-windowYPos))*screenRatio);
            
                float blur = _Blur;
                blur = max(1, blur);

                fixed4 col = fixed4(0, 0, 0, 0);
                fixed4 windowCol = tex2Dproj(_GrabTexture, i.grabPos);
                float weight_total = 0;

                [loop]
                for (float y = -blur; y <= blur; y += 1)
                {
                    float weight = (1 / (_StandardDeviation * sqrt(2 * 3.14159265359)) * exp(-pow(y, 2)/(2 * pow(_StandardDeviation, 2))));
                    weight_total += weight;
                    col += tex2Dproj(_GrabTexture, i.grabPos + float4(0, y * _GrabTexture_TexelSize.y, 0, 0)) * weight;
                }
                col /= weight_total;

                if (L < _WindowSize)
                    opacity = 0;
                else
                    opacity = 1;
                
                col = windowCol * (1 - opacity) + col * (opacity);

                return col;
            }
            ENDCG
        }
    }
}