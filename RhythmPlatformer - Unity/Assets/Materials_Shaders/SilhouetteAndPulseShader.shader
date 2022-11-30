Shader "Custom/SilhouetteAndPulseShader"
{
    Properties
    {
        [MainTexture] [NoScaleOffset] _MainTex("Main", 2D) = "white" {}
        _ScrollSpeed("ScrollSpeed", Float) = 0.05
        _MaskRange("MaskRange", Float) = 0
        _BaseColor("BaseColor", Color) = (0.9254902, 0.2941177, 0.9333333, 0)
        [HDR]_SecondaryColor("SecondaryColor", Color) = (3.078862, 0.3877086, 4.35602, 0)
        _Fuzziness("Fuzziness", Float) = 0

        _Resolution("Resolution", Float) = 0.001
        _OutlineThickness("Outline Thickness", Float) = 0.5
        _FadeThickness("Fade Thickness", Range(0, 20)) = 20
        _AlphaFadeThickness("Alpha Fade Thickness", Float) = 75
        _CenterColor("Center Color", Color) = (1, 1, 1, 1)
        [HDR]_EdgeColor("Edge Color", Color) = (1, 1, 1, 1)

        [HideInInspector]_FlipX ("Flip X", Float) = 0

        [HideInInspector]_BUILTIN_QueueOffset("Float", Float) = 0
        [HideInInspector]_BUILTIN_QueueControl("Float", Float) = -1
    }

    SubShader
    {
        Tags
        {
            // RenderPipeline: <None>
            "RenderType" = "Transparent"
            "BuiltInMaterialType" = "Unlit"
            "Queue" = "Transparent"
            "ShaderGraphShader" = "true"
            "ShaderGraphTargetId" = "BuiltInUnlitSubTarget"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        // ECHO (LEFT) ------

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define DIV_SQRT_2 0.70710678118
            #define DIV16_LONG_SIDE 0.8944272
            #define DIV16_SHORT_SIDE 0.4472136

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Resolution;
            int _FadeThickness;
            float _AlphaFadeThickness;
            float _OutlineThickness;
            float4 _EdgeColor;
            float4 _CenterColor;
            float _FlipX;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(float4(v.vertex.x - _OutlineThickness, v.vertex.yzw));
                o.uv = TRANSFORM_TEX(v.uv0, _MainTex);

                return o;
            }

            float InverseLerp(float a, float b, float v) {
                return (v - a) / (b - a);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 directions[16] = {float2(1, 0), float2(0, 1), float2(-1, 0), float2(0, -1),
                    float2(DIV_SQRT_2, DIV_SQRT_2), float2(-DIV_SQRT_2, DIV_SQRT_2),
                    float2(-DIV_SQRT_2, -DIV_SQRT_2), float2(DIV_SQRT_2, -DIV_SQRT_2),
                    float2(DIV16_LONG_SIDE, DIV16_SHORT_SIDE), float2(DIV16_SHORT_SIDE, DIV16_LONG_SIDE),
                    float2(-DIV16_LONG_SIDE, DIV16_SHORT_SIDE), float2(-DIV16_SHORT_SIDE, DIV16_LONG_SIDE),
                    float2(DIV16_LONG_SIDE, -DIV16_SHORT_SIDE), float2(DIV16_SHORT_SIDE, -DIV16_LONG_SIDE),
                    float2(-DIV16_LONG_SIDE, -DIV16_SHORT_SIDE), float2(-DIV16_SHORT_SIDE, -DIV16_LONG_SIDE)
                };

                float loopsToEdge = _FadeThickness;

                for (uint loopCount = 0; loopCount < _FadeThickness; loopCount++) {
                    for (uint index = 0; index < 16; index++) {
                        float2 sampleUV = i.uv + directions[index] * (loopCount + 1) * _Resolution;
                        if (tex2D(_MainTex, sampleUV).a <= 0) {
                            loopsToEdge = min(loopsToEdge, loopCount);
                        }
                    }
                }

                // NOTE: calculating the alpha depends on how the single-frame UV is laid out:
                // the number of columns in used to locate the frame's uv within the atlas's uv

                float columnWidth = 0.25;

                uint column = floor(i.uv.x / columnWidth);
                float xPosInColumn = (i.uv.x - column * columnWidth) / columnWidth;
                float columnMaxX = i.uv.x - (i.uv.x - column * columnWidth) + columnWidth * (1 - _FlipX);

                float farthestTransparentSample = 0;
                float targetEdgeX = 0;
                for (float loop = 0; loop < columnWidth / _Resolution; loop++) {
                    float2 sampleUV = float2(columnMaxX + (-1 + 2 * _FlipX) * loop * _Resolution, i.uv.y);
                    float sampleAlpha = tex2D(_MainTex, sampleUV).a;

                    farthestTransparentSample += (sampleAlpha <= 0) * (sampleUV.x - farthestTransparentSample);

                    float checkGreater = max(_FlipX + (farthestTransparentSample > i.uv.x), 1);
                    float checkLess = max((1 - _FlipX) + (farthestTransparentSample < i.uv.x), 1);
                    targetEdgeX += (sampleAlpha > 0) * checkGreater *  checkLess * (sampleUV.x - targetEdgeX);
                }

                float alpha = saturate(tex2D(_MainTex, i.uv).a - abs(targetEdgeX - i.uv.x) * _AlphaFadeThickness);

                float distToEdge = InverseLerp(_FadeThickness, 0, loopsToEdge);
                float4 color = lerp(_CenterColor, _EdgeColor, distToEdge);

                return float4(distToEdge.xxx * color, alpha);
            }
            ENDCG
        }

        // ECHO (RIGHT) ------

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define DIV_SQRT_2 0.70710678118
            #define DIV16_LONG_SIDE 0.8944272
            #define DIV16_SHORT_SIDE 0.4472136

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Resolution;
            int _FadeThickness;
            float _AlphaFadeThickness;
            float _OutlineThickness;
            float4 _EdgeColor;
            float4 _CenterColor;
            float _FlipX;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(float4(v.vertex.x + _OutlineThickness, v.vertex.yzw));
                o.uv = TRANSFORM_TEX(v.uv0, _MainTex);

                return o;
            }

            float InverseLerp(float a, float b, float v) {
                return (v - a) / (b - a);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 directions[16] = {float2(1, 0), float2(0, 1), float2(-1, 0), float2(0, -1),
                    float2(DIV_SQRT_2, DIV_SQRT_2), float2(-DIV_SQRT_2, DIV_SQRT_2),
                    float2(-DIV_SQRT_2, -DIV_SQRT_2), float2(DIV_SQRT_2, -DIV_SQRT_2),
                    float2(DIV16_LONG_SIDE, DIV16_SHORT_SIDE), float2(DIV16_SHORT_SIDE, DIV16_LONG_SIDE),
                    float2(-DIV16_LONG_SIDE, DIV16_SHORT_SIDE), float2(-DIV16_SHORT_SIDE, DIV16_LONG_SIDE),
                    float2(DIV16_LONG_SIDE, -DIV16_SHORT_SIDE), float2(DIV16_SHORT_SIDE, -DIV16_LONG_SIDE),
                    float2(-DIV16_LONG_SIDE, -DIV16_SHORT_SIDE), float2(-DIV16_SHORT_SIDE, -DIV16_LONG_SIDE)
                };

                float loopsToEdge = _FadeThickness;

                for (uint loopCount = 0; loopCount < _FadeThickness; loopCount++) {
                    for (uint index = 0; index < 16; index++) {
                        float2 sampleUV = i.uv + directions[index] * (loopCount + 1) * _Resolution;
                        if (tex2D(_MainTex, sampleUV).a <= 0) {
                            loopsToEdge = min(loopsToEdge, loopCount);
                        }
                    }
                }

                // NOTE: calculating the alpha depends on how the single-frame UV is laid out:
                // the number of columns in used to locate the frame's uv within the atlas's uv

                float columnWidth = 0.25;

                uint column = floor(i.uv.x / columnWidth);
                float xPosInColumn = (i.uv.x - column * columnWidth) / columnWidth;
                float columnMinX = i.uv.x - (i.uv.x - column * columnWidth) + columnWidth * _FlipX;

                float farthestTransparentSample = 0;
                float targetEdgeX = 0;
                for (float loop = 0; loop < columnWidth / _Resolution; loop++) {
                    float2 sampleUV = float2(columnMinX + (1 - 2 * _FlipX) * loop * _Resolution, i.uv.y);
                    float sampleAlpha = tex2D(_MainTex, sampleUV).a;

                    farthestTransparentSample += (sampleAlpha <= 0) * (sampleUV.x - farthestTransparentSample);

                    float checkGreater = max((1 - _FlipX) + (farthestTransparentSample > i.uv.x), 1);
                    float checkLess = max(_FlipX + (farthestTransparentSample < i.uv.x), 1);
                    targetEdgeX += (sampleAlpha > 0) * checkGreater * checkLess * (sampleUV.x - targetEdgeX);
                }

                float alpha = saturate(tex2D(_MainTex, i.uv).a - abs(targetEdgeX - i.uv.x) * _AlphaFadeThickness);

                float distToEdge = InverseLerp(_FadeThickness, 0, loopsToEdge);
                float4 color = lerp(_CenterColor, _EdgeColor, distToEdge);

                return float4(distToEdge.xxx * color, alpha);
            }
            ENDCG
        }

        Pass
        {
            Name "Pass"
            Tags
            {
                "LightMode" = "ForwardBase"
            }

            // Render State
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            ZTest LEqual
            ZWrite Off
            ColorMask RGB

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 3.0
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma vertex vert
            #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_UNLIT
            #define BUILTIN_TARGET_API 1
            #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
            #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
            #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
            #endif
            #ifdef _BUILTIN_ALPHATEST_ON
            #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
            #endif
            #ifdef _BUILTIN_AlphaClip
            #define _AlphaClip _BUILTIN_AlphaClip
            #endif
            #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
            #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
            #endif


            // custom interpolator pre-include
            /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */

            // Includes
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            // custom interpolators pre packing
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */

            struct Attributes
            {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
            struct Varyings
            {
                    float4 positionCS : SV_POSITION;
                    float4 texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            struct SurfaceDescriptionInputs
            {
                    float4 uv0;
                    float3 TimeParameters;
            };
            struct VertexDescriptionInputs
            {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
            };
            struct PackedVaryings
            {
                    float4 positionCS : SV_POSITION;
                    float4 interp0 : INTERP0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                ZERO_INITIALIZE(PackedVaryings, output);
                output.positionCS = input.positionCS;
                output.interp0.xyzw = input.texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }

            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output;
                output.positionCS = input.positionCS;
                output.texCoord0 = input.interp0.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }


            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
            float _ScrollSpeed;
            float4 _SecondaryColor;
            float4 _BaseColor;
            float _Fuzziness;
            float _MaskRange;
            CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif

        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif

        // Graph Includes
        // GraphIncludes: <None>

        // Graph Functions

        void Unity_ColorMask_float(float3 In, float3 MaskColor, float Range, out float Out, float Fuzziness)
        {
            float Distance = distance(MaskColor, In);
            Out = saturate(1 - (Distance - Range) / max(Fuzziness, 1e-5));
        }

        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }

        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }

        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }

        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }


        float2 Unity_GradientNoise_Dir_float(float2 p)
        {
            // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
            p = p % 289;
            // need full precision, otherwise half overflows when p > 1
            float x = float(34 * p.x + 1) * p.x % 289 + p.y;
            x = (34 * x + 1) * x % 289;
            x = frac(x / 41) * 2 - 1;
            return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
        }

        void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
        {
            float2 p = UV * Scale;
            float2 ip = floor(p);
            float2 fp = frac(p);
            float d00 = dot(Unity_GradientNoise_Dir_float(ip), fp);
            float d01 = dot(Unity_GradientNoise_Dir_float(ip + float2(0, 1)), fp - float2(0, 1));
            float d10 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 0)), fp - float2(1, 0));
            float d11 = dot(Unity_GradientNoise_Dir_float(ip + float2(1, 1)), fp - float2(1, 1));
            fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
            Out = lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
        }

        void Unity_OneMinus_float4(float4 In, out float4 Out)
        {
            Out = 1 - In;
        }

        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }

        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }

        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }

        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */

        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif

        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float4 _Property_eb53d1f7e49845bc81e5cabd327c14b6_Out_0 = _BaseColor;
            UnityTexture2D _Property_2c0e083e23dc4446971eedc747bb9127_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2c0e083e23dc4446971eedc747bb9127_Out_0.tex, _Property_2c0e083e23dc4446971eedc747bb9127_Out_0.samplerstate, _Property_2c0e083e23dc4446971eedc747bb9127_Out_0.GetTransformedUV(IN.uv0.xy));
            float _SampleTexture2D_340838948c6340f8a057e55938730dbb_R_4 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.r;
            float _SampleTexture2D_340838948c6340f8a057e55938730dbb_G_5 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.g;
            float _SampleTexture2D_340838948c6340f8a057e55938730dbb_B_6 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.b;
            float _SampleTexture2D_340838948c6340f8a057e55938730dbb_A_7 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.a;
            float3 _Vector3_c44fafafdfd74b87a99c44ec99a06a76_Out_0 = float3(_SampleTexture2D_340838948c6340f8a057e55938730dbb_R_4, _SampleTexture2D_340838948c6340f8a057e55938730dbb_G_5, _SampleTexture2D_340838948c6340f8a057e55938730dbb_B_6);
            float _Property_81abe2cd4e2646db89cbb7e4a630acab_Out_0 = _MaskRange;
            float _Property_475f55c395e54ce5acb40d59ec46c8d9_Out_0 = _Fuzziness;
            float _ColorMask_fd719d7c9839439e986a2ebb00140315_Out_3;
            Unity_ColorMask_float(_Vector3_c44fafafdfd74b87a99c44ec99a06a76_Out_0, IsGammaSpace() ? float3(1, 1, 1) : SRGBToLinear(float3(1, 1, 1)), _Property_81abe2cd4e2646db89cbb7e4a630acab_Out_0, _ColorMask_fd719d7c9839439e986a2ebb00140315_Out_3, _Property_475f55c395e54ce5acb40d59ec46c8d9_Out_0);
            float4 _Multiply_3fa0b360dc5c40a4b1dd3c3827a7e025_Out_2;
            Unity_Multiply_float4_float4(_Property_eb53d1f7e49845bc81e5cabd327c14b6_Out_0, (_ColorMask_fd719d7c9839439e986a2ebb00140315_Out_3.xxxx), _Multiply_3fa0b360dc5c40a4b1dd3c3827a7e025_Out_2);
            float4 _Property_9554def5e8934065b0eefa0278a2f0f7_Out_0 = IsGammaSpace() ? LinearToSRGB(_SecondaryColor) : _SecondaryColor;
            float4 _Add_7d4bb846d2294a5f82ccc9158ceeddae_Out_2;
            Unity_Add_float4((_ColorMask_fd719d7c9839439e986a2ebb00140315_Out_3.xxxx), _Property_9554def5e8934065b0eefa0278a2f0f7_Out_0, _Add_7d4bb846d2294a5f82ccc9158ceeddae_Out_2);
            float4 _Multiply_133c2d02c4aa497ca36c66d667c89171_Out_2;
            Unity_Multiply_float4_float4(_Multiply_3fa0b360dc5c40a4b1dd3c3827a7e025_Out_2, _Add_7d4bb846d2294a5f82ccc9158ceeddae_Out_2, _Multiply_133c2d02c4aa497ca36c66d667c89171_Out_2);
            float _Property_d9eef888d51d406790369bb42f9365eb_Out_0 = _ScrollSpeed;
            float _Multiply_df134f0d99e740959d79c44377bb7a5c_Out_2;
            Unity_Multiply_float_float(_Property_d9eef888d51d406790369bb42f9365eb_Out_0, IN.TimeParameters.x, _Multiply_df134f0d99e740959d79c44377bb7a5c_Out_2);
            float _Property_37e23eeb91c34dde801650d73d9367b2_Out_0 = _ScrollSpeed;
            float _Multiply_da84d05aee5445098f3d12f2588f2373_Out_2;
            Unity_Multiply_float_float(IN.TimeParameters.z, _Property_37e23eeb91c34dde801650d73d9367b2_Out_0, _Multiply_da84d05aee5445098f3d12f2588f2373_Out_2);
            float2 _Vector2_dae9124b9e39420681632cb97f2f6995_Out_0 = float2(_Multiply_df134f0d99e740959d79c44377bb7a5c_Out_2, _Multiply_da84d05aee5445098f3d12f2588f2373_Out_2);
            float2 _TilingAndOffset_d63ff126842344f187527e4e6f66f1a4_Out_3;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), _Vector2_dae9124b9e39420681632cb97f2f6995_Out_0, _TilingAndOffset_d63ff126842344f187527e4e6f66f1a4_Out_3);
            float _GradientNoise_01fe4f11c93c4d85a89e9d45ea43900f_Out_2;
            Unity_GradientNoise_float(_TilingAndOffset_d63ff126842344f187527e4e6f66f1a4_Out_3, 10, _GradientNoise_01fe4f11c93c4d85a89e9d45ea43900f_Out_2);
            float4 Color_eeca5938061747129044c755de65d929 = IsGammaSpace() ? float4(0.6226415, 0.6226415, 0.6226415, 1) : float4(SRGBToLinear(float3(0.6226415, 0.6226415, 0.6226415)), 1);
            float4 _Multiply_afed7556254b41ae802300cec242635d_Out_2;
            Unity_Multiply_float4_float4((_GradientNoise_01fe4f11c93c4d85a89e9d45ea43900f_Out_2.xxxx), Color_eeca5938061747129044c755de65d929, _Multiply_afed7556254b41ae802300cec242635d_Out_2);
            float4 _OneMinus_9c1a7337cbd44227a3acaa657b874d33_Out_1;
            Unity_OneMinus_float4(_Multiply_afed7556254b41ae802300cec242635d_Out_2, _OneMinus_9c1a7337cbd44227a3acaa657b874d33_Out_1);
            float4 _Multiply_2883af9ca8924e48b32a56555e22258e_Out_2;
            Unity_Multiply_float4_float4(_Multiply_133c2d02c4aa497ca36c66d667c89171_Out_2, _OneMinus_9c1a7337cbd44227a3acaa657b874d33_Out_1, _Multiply_2883af9ca8924e48b32a56555e22258e_Out_2);
            float _OneMinus_89128aeb137b476a912314f677794583_Out_1;
            Unity_OneMinus_float(_ColorMask_fd719d7c9839439e986a2ebb00140315_Out_3, _OneMinus_89128aeb137b476a912314f677794583_Out_1);
            float3 _Multiply_1039681f6bfd4bc8b2d5e68dfb4cb945_Out_2;
            Unity_Multiply_float3_float3(_Vector3_c44fafafdfd74b87a99c44ec99a06a76_Out_0, (_OneMinus_89128aeb137b476a912314f677794583_Out_1.xxx), _Multiply_1039681f6bfd4bc8b2d5e68dfb4cb945_Out_2);
            float3 _Add_0e6c41656aa7461d991f679de9ae4464_Out_2;
            Unity_Add_float3((_Multiply_2883af9ca8924e48b32a56555e22258e_Out_2.xyz), _Multiply_1039681f6bfd4bc8b2d5e68dfb4cb945_Out_2, _Add_0e6c41656aa7461d991f679de9ae4464_Out_2);
            surface.BaseColor = _Add_0e6c41656aa7461d991f679de9ae4464_Out_2;
            surface.Alpha = _SampleTexture2D_340838948c6340f8a057e55938730dbb_A_7;
            return surface;
        }

        // --------------------------------------------------
        // Build Graph Inputs

        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal = input.normalOS;
            output.ObjectSpaceTangent = input.tangentOS.xyz;
            output.ObjectSpacePosition = input.positionOS;

            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);







            output.uv0 = input.texCoord0;
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

                return output;
        }

        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex = float4(attributes.positionOS, 1);
            result.tangent = attributes.tangentOS;
            result.normal = attributes.normalOS;
            result.texcoord = attributes.uv0;
            result.vertex = float4(vertexDescription.Position, 1);
            result.normal = vertexDescription.Normal;
            result.tangent = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }

        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf


            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif

            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }

        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf

            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif

            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }

        // --------------------------------------------------
        // Main

        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/UnlitPass.hlsl"

        ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // Render State
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            ZTest LEqual
            ZWrite On
            ColorMask 0

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 3.0
            #pragma multi_compile_shadowcaster
            #pragma vertex vert
            #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            // GraphKeywords: <None>

            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS SHADERPASS_SHADOWCASTER
            #define BUILTIN_TARGET_API 1
            #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
            #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
            #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
            #endif
            #ifdef _BUILTIN_ALPHATEST_ON
            #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
            #endif
            #ifdef _BUILTIN_AlphaClip
            #define _AlphaClip _BUILTIN_AlphaClip
            #endif
            #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
            #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
            #endif


            // custom interpolator pre-include
            /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */

            // Includes
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            // custom interpolators pre packing
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */

            struct Attributes
            {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
            struct Varyings
            {
                    float4 positionCS : SV_POSITION;
                    float4 texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            struct SurfaceDescriptionInputs
            {
                    float4 uv0;
            };
            struct VertexDescriptionInputs
            {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
            };
            struct PackedVaryings
            {
                    float4 positionCS : SV_POSITION;
                    float4 interp0 : INTERP0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                ZERO_INITIALIZE(PackedVaryings, output);
                output.positionCS = input.positionCS;
                output.interp0.xyzw = input.texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }

            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output;
                output.positionCS = input.positionCS;
                output.texCoord0 = input.interp0.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }


            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
            float _ScrollSpeed;
            float4 _SecondaryColor;
            float4 _BaseColor;
            float _Fuzziness;
            float _MaskRange;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // -- Property used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif

            // -- Properties used by SceneSelectionPass
            #ifdef SCENESELECTIONPASS
            int _ObjectId;
            int _PassValue;
            #endif

            // Graph Includes
            // GraphIncludes: <None>

            // Graph Functions
            // GraphFunctions: <None>

            // Custom interpolators pre vertex
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */

            // Graph Vertex
            struct VertexDescription
            {
                float3 Position;
                float3 Normal;
                float3 Tangent;
            };

            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                description.Position = IN.ObjectSpacePosition;
                description.Normal = IN.ObjectSpaceNormal;
                description.Tangent = IN.ObjectSpaceTangent;
                return description;
            }

            // Custom interpolators, pre surface
            #ifdef FEATURES_GRAPH_VERTEX
            Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
            {
            return output;
            }
            #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
            #endif

            // Graph Pixel
            struct SurfaceDescription
            {
                float Alpha;
            };

            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                UnityTexture2D _Property_2c0e083e23dc4446971eedc747bb9127_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
                float4 _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2c0e083e23dc4446971eedc747bb9127_Out_0.tex, _Property_2c0e083e23dc4446971eedc747bb9127_Out_0.samplerstate, _Property_2c0e083e23dc4446971eedc747bb9127_Out_0.GetTransformedUV(IN.uv0.xy));
                float _SampleTexture2D_340838948c6340f8a057e55938730dbb_R_4 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.r;
                float _SampleTexture2D_340838948c6340f8a057e55938730dbb_G_5 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.g;
                float _SampleTexture2D_340838948c6340f8a057e55938730dbb_B_6 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.b;
                float _SampleTexture2D_340838948c6340f8a057e55938730dbb_A_7 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.a;
                surface.Alpha = _SampleTexture2D_340838948c6340f8a057e55938730dbb_A_7;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                output.ObjectSpaceNormal = input.normalOS;
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                output.ObjectSpacePosition = input.positionOS;

                return output;
            }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);







                output.uv0 = input.texCoord0;
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
            #else
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            #endif
            #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

                    return output;
            }

            void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
            {
                result.vertex = float4(attributes.positionOS, 1);
                result.tangent = attributes.tangentOS;
                result.normal = attributes.normalOS;
                result.texcoord = attributes.uv0;
                result.vertex = float4(vertexDescription.Position, 1);
                result.normal = vertexDescription.Normal;
                result.tangent = float4(vertexDescription.Tangent, 0);
                #if UNITY_ANY_INSTANCING_ENABLED
                #endif
            }

            void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
            {
                result.pos = varyings.positionCS;
                // World Tangent isn't an available input on v2f_surf


                #if UNITY_ANY_INSTANCING_ENABLED
                #endif
                #if !defined(LIGHTMAP_ON)
                #if UNITY_SHOULD_SAMPLE_SH
                #endif
                #endif
                #if defined(LIGHTMAP_ON)
                #endif
                #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                    result.fogCoord = varyings.fogFactorAndVertexLight.x;
                    COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
                #endif

                DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
            }

            void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
            {
                result.positionCS = surfVertex.pos;
                // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
                // World Tangent isn't an available input on v2f_surf

                #if UNITY_ANY_INSTANCING_ENABLED
                #endif
                #if !defined(LIGHTMAP_ON)
                #if UNITY_SHOULD_SAMPLE_SH
                #endif
                #endif
                #if defined(LIGHTMAP_ON)
                #endif
                #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                    result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                    COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
                #endif

                DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
            }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

            ENDHLSL
            }
            Pass
            {
                Name "SceneSelectionPass"
                Tags
                {
                    "LightMode" = "SceneSelectionPass"
                }

                // Render State
                Cull Off

                // Debug
                // <None>

                // --------------------------------------------------
                // Pass

                HLSLPROGRAM

                // Pragmas
                #pragma target 3.0
                #pragma multi_compile_instancing
                #pragma vertex vert
                #pragma fragment frag

                // DotsInstancingOptions: <None>
                // HybridV1InjectedBuiltinProperties: <None>

                // Keywords
                // PassKeywords: <None>
                // GraphKeywords: <None>

                // Defines
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define VARYINGS_NEED_TEXCOORD0
                #define FEATURES_GRAPH_VERTEX
                /* WARNING: $splice Could not find named fragment 'PassInstancing' */
                #define SHADERPASS SceneSelectionPass
                #define BUILTIN_TARGET_API 1
                #define SCENESELECTIONPASS 1
                #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
                /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
                #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
                #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
                #endif
                #ifdef _BUILTIN_ALPHATEST_ON
                #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
                #endif
                #ifdef _BUILTIN_AlphaClip
                #define _AlphaClip _BUILTIN_AlphaClip
                #endif
                #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
                #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
                #endif


                // custom interpolator pre-include
                /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */

                // Includes
                #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
                #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
                #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
                #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"

                // --------------------------------------------------
                // Structs and Packing

                // custom interpolators pre packing
                /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */

                struct Attributes
                {
                        float3 positionOS : POSITION;
                        float3 normalOS : NORMAL;
                        float4 tangentOS : TANGENT;
                        float4 uv0 : TEXCOORD0;
                    #if UNITY_ANY_INSTANCING_ENABLED
                        uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct Varyings
                {
                        float4 positionCS : SV_POSITION;
                        float4 texCoord0;
                    #if UNITY_ANY_INSTANCING_ENABLED
                        uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                        float4 uv0;
                };
                struct VertexDescriptionInputs
                {
                        float3 ObjectSpaceNormal;
                        float3 ObjectSpaceTangent;
                        float3 ObjectSpacePosition;
                };
                struct PackedVaryings
                {
                        float4 positionCS : SV_POSITION;
                        float4 interp0 : INTERP0;
                    #if UNITY_ANY_INSTANCING_ENABLED
                        uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                        uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                        uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                        FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };

                PackedVaryings PackVaryings(Varyings input)
                {
                    PackedVaryings output;
                    ZERO_INITIALIZE(PackedVaryings, output);
                    output.positionCS = input.positionCS;
                    output.interp0.xyzw = input.texCoord0;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }

                Varyings UnpackVaryings(PackedVaryings input)
                {
                    Varyings output;
                    output.positionCS = input.positionCS;
                    output.texCoord0 = input.interp0.xyzw;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }


                // --------------------------------------------------
                // Graph

                // Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;
                float _ScrollSpeed;
                float4 _SecondaryColor;
                float4 _BaseColor;
                float _Fuzziness;
                float _MaskRange;
                CBUFFER_END

        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif

        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif

        // Graph Includes
        // GraphIncludes: <None>

        // Graph Functions
        // GraphFunctions: <None>

        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */

        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };

        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }

        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif

        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };

        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_2c0e083e23dc4446971eedc747bb9127_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2c0e083e23dc4446971eedc747bb9127_Out_0.tex, _Property_2c0e083e23dc4446971eedc747bb9127_Out_0.samplerstate, _Property_2c0e083e23dc4446971eedc747bb9127_Out_0.GetTransformedUV(IN.uv0.xy));
            float _SampleTexture2D_340838948c6340f8a057e55938730dbb_R_4 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.r;
            float _SampleTexture2D_340838948c6340f8a057e55938730dbb_G_5 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.g;
            float _SampleTexture2D_340838948c6340f8a057e55938730dbb_B_6 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.b;
            float _SampleTexture2D_340838948c6340f8a057e55938730dbb_A_7 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.a;
            surface.Alpha = _SampleTexture2D_340838948c6340f8a057e55938730dbb_A_7;
            return surface;
        }

        // --------------------------------------------------
        // Build Graph Inputs

        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);

            output.ObjectSpaceNormal = input.normalOS;
            output.ObjectSpaceTangent = input.tangentOS.xyz;
            output.ObjectSpacePosition = input.positionOS;

            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);







            output.uv0 = input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

                return output;
        }

        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex = float4(attributes.positionOS, 1);
            result.tangent = attributes.tangentOS;
            result.normal = attributes.normalOS;
            result.texcoord = attributes.uv0;
            result.vertex = float4(vertexDescription.Position, 1);
            result.normal = vertexDescription.Normal;
            result.tangent = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }

        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf


            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif

            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }

        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf

            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if !defined(LIGHTMAP_ON)
            #if UNITY_SHOULD_SAMPLE_SH
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif

            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }

        // --------------------------------------------------
        // Main

        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }

            // Render State
            Cull Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
            #pragma target 3.0
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>

            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD0
            #define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
            #define SHADERPASS ScenePickingPass
            #define BUILTIN_TARGET_API 1
            #define SCENEPICKINGPASS 1
            #define _BUILTIN_SURFACE_TYPE_TRANSPARENT 1
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
            #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
            #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
            #endif
            #ifdef _BUILTIN_ALPHATEST_ON
            #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
            #endif
            #ifdef _BUILTIN_AlphaClip
            #define _AlphaClip _BUILTIN_AlphaClip
            #endif
            #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
            #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
            #endif


            // custom interpolator pre-include
            /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */

            // Includes
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            // custom interpolators pre packing
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */

            struct Attributes
            {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
            struct Varyings
            {
                    float4 positionCS : SV_POSITION;
                    float4 texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };
            struct SurfaceDescriptionInputs
            {
                    float4 uv0;
            };
            struct VertexDescriptionInputs
            {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
            };
            struct PackedVaryings
            {
                    float4 positionCS : SV_POSITION;
                    float4 interp0 : INTERP0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                ZERO_INITIALIZE(PackedVaryings, output);
                output.positionCS = input.positionCS;
                output.interp0.xyzw = input.texCoord0;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }

            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output;
                output.positionCS = input.positionCS;
                output.texCoord0 = input.interp0.xyzw;
                #if UNITY_ANY_INSTANCING_ENABLED
                output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                output.cullFace = input.cullFace;
                #endif
                return output;
            }


            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
            float _ScrollSpeed;
            float4 _SecondaryColor;
            float4 _BaseColor;
            float _Fuzziness;
            float _MaskRange;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // -- Property used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif

            // -- Properties used by SceneSelectionPass
            #ifdef SCENESELECTIONPASS
            int _ObjectId;
            int _PassValue;
            #endif

            // Graph Includes
            // GraphIncludes: <None>

            // Graph Functions
            // GraphFunctions: <None>

            // Custom interpolators pre vertex
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */

            // Graph Vertex
            struct VertexDescription
            {
                float3 Position;
                float3 Normal;
                float3 Tangent;
            };

            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                description.Position = IN.ObjectSpacePosition;
                description.Normal = IN.ObjectSpaceNormal;
                description.Tangent = IN.ObjectSpaceTangent;
                return description;
            }

            // Custom interpolators, pre surface
            #ifdef FEATURES_GRAPH_VERTEX
            Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
            {
            return output;
            }
            #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
            #endif

            // Graph Pixel
            struct SurfaceDescription
            {
                float Alpha;
            };

            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                UnityTexture2D _Property_2c0e083e23dc4446971eedc747bb9127_Out_0 = UnityBuildTexture2DStructNoScale(_MainTex);
                float4 _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2c0e083e23dc4446971eedc747bb9127_Out_0.tex, _Property_2c0e083e23dc4446971eedc747bb9127_Out_0.samplerstate, _Property_2c0e083e23dc4446971eedc747bb9127_Out_0.GetTransformedUV(IN.uv0.xy));
                float _SampleTexture2D_340838948c6340f8a057e55938730dbb_R_4 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.r;
                float _SampleTexture2D_340838948c6340f8a057e55938730dbb_G_5 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.g;
                float _SampleTexture2D_340838948c6340f8a057e55938730dbb_B_6 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.b;
                float _SampleTexture2D_340838948c6340f8a057e55938730dbb_A_7 = _SampleTexture2D_340838948c6340f8a057e55938730dbb_RGBA_0.a;
                surface.Alpha = _SampleTexture2D_340838948c6340f8a057e55938730dbb_A_7;
                return surface;
            }

            // --------------------------------------------------
            // Build Graph Inputs

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                output.ObjectSpaceNormal = input.normalOS;
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                output.ObjectSpacePosition = input.positionOS;

                return output;
            }
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);







                output.uv0 = input.texCoord0;
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
            #else
            #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            #endif
            #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

                    return output;
            }

            void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
            {
                result.vertex = float4(attributes.positionOS, 1);
                result.tangent = attributes.tangentOS;
                result.normal = attributes.normalOS;
                result.texcoord = attributes.uv0;
                result.vertex = float4(vertexDescription.Position, 1);
                result.normal = vertexDescription.Normal;
                result.tangent = float4(vertexDescription.Tangent, 0);
                #if UNITY_ANY_INSTANCING_ENABLED
                #endif
            }

            void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
            {
                result.pos = varyings.positionCS;
                // World Tangent isn't an available input on v2f_surf


                #if UNITY_ANY_INSTANCING_ENABLED
                #endif
                #if !defined(LIGHTMAP_ON)
                #if UNITY_SHOULD_SAMPLE_SH
                #endif
                #endif
                #if defined(LIGHTMAP_ON)
                #endif
                #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                    result.fogCoord = varyings.fogFactorAndVertexLight.x;
                    COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
                #endif

                DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
            }

            void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
            {
                result.positionCS = surfVertex.pos;
                // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
                // World Tangent isn't an available input on v2f_surf

                #if UNITY_ANY_INSTANCING_ENABLED
                #endif
                #if !defined(LIGHTMAP_ON)
                #if UNITY_SHOULD_SAMPLE_SH
                #endif
                #endif
                #if defined(LIGHTMAP_ON)
                #endif
                #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                    result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                    COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
                #endif

                DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
            }

            // --------------------------------------------------
            // Main

            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

            ENDHLSL
        }
    }
    CustomEditorForRenderPipeline "UnityEditor.Rendering.BuiltIn.ShaderGraph.BuiltInUnlitGUI" ""
                                            CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
                                            FallBack "Hidden/Shader Graph/FallbackError"
}