#ifndef DARK_SHADER_FUNCTIONS
#define DARK_SHADER_FUNCTIONS

half _Dark;
half4 _DarkColor;
sampler2D _DarkNoise;
sampler2D _DarkLUT;

half4 SampleDark(float2 uv) {
    return tex2D(_DarkNoise, uv);
}


half4 DefaultDoubleSampleDark(float2 screenuv, float ratio1, float ratio2) {
    return SampleDark((screenuv + _Time.x) / 2) * ratio1
        + SampleDark(screenuv - _Time.x) * ratio2;
}

half4 DefaultDoubleSampleDark(float2 screenuv) {
    return DefaultDoubleSampleDark(screenuv, 0.5, 0.5);
}

half4 MixDark(half4 color, float2 screenuv) {
    half dVal = DefaultDoubleSampleDark(screenuv).r;
    half4 dark = tex2D(_DarkLUT, float2(dVal, 0)) * _DarkColor;

    half2 suvn = (screenuv - 0.5) * 2;
    half fact = 2*smoothstep(0.9, 1.1, length(suvn));
    dark.rgb *= 1 + fact;

    half darkFactor = dark.a * _Dark;
    color.rgb = darkFactor * dark.rgb + (1 - darkFactor) * color.rgb;
    return color;
}

float random(float2 st) {
    return frac(sin(dot(st.xy,
        float2(12.9898, 78.233))) *
        43758.5453123);
}

#endif 