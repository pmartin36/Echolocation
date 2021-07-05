#ifndef DARK_SHADER_FUNCTIONS
#define DARK_SHADER_FUNCTIONS

half _Dark;
half4 _DarkColor;
sampler2D _DarkNoise;
sampler2D _DarkLUT;

half4 MixDark(half4 color, float2 screenuv) {
    half dVal = tex2D(_DarkNoise, (screenuv + _Time.x) / 2).r
        + tex2D(_DarkNoise, screenuv - _Time.x);
    half4 dark = tex2D(_DarkLUT, float2(dVal/2, 0)) * _DarkColor;

    half2 suvn = (screenuv - 0.5) * 2;
    half fact = 2*smoothstep(0.9, 1.1, length(suvn));
    dark.rgb *= 1 + fact;

    half darkFactor = dark.a * _Dark;
    color.rgb = darkFactor * dark.rgb + (1 - darkFactor) * color.rgb;
    return color;
}

#endif 