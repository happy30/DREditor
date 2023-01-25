Shader "Custom/OverlayBlend" 
{
Properties {
    _MainTex ("Texture1 (RGB)", 2D) = "white" {}
    _Color ("Main Color", Color) = (1,1,1,1)
}

Category {
    Tags {"RenderType"="Opaque" "Queue"="Transparent"}
    Lighting Off
    BindChannels {
        Bind "Color", color
        Bind "Vertex", vertex
        Bind "Texcoord", Texcoord
    }

    //screen 1 - (1 - a) * (1 - b)

    SubShader {
        Pass {
            ZWrite Off
            Cull Off
            Blend One One
            BlendOp Sub
            SetTexture [_MainTex] {
                constantColor (1,1,1,1)
                Combine constant
            }
        }

        Pass {
            ZWrite Off
            Cull Off
            Blend Zero OneMinusSrcColor
            BlendOp Add
            SetTexture [_MainTex] {
                constantColor [_Color]
                Combine texture * constant
            }
        }
        Pass {
            ZWrite Off
            Cull Off
            Blend One One
            BlendOp Sub
            SetTexture [_MainTex] {
                constantColor [_Color]
            }
            SetTexture [_MainTex] {
                constantColor (1,1,1,1)
                Combine constant, previous
            }
        }
    }
}
}