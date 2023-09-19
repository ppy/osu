#define HIGH_PRECISION_VERTEX

#include "sh_Utils.h"
#include "sh_Masking.h"

layout(location = 2) in highp vec2 v_TexCoord;

layout(location = 0) out vec4 o_Colour;

void main(void)
{
    highp float hueValue = v_TexCoord.x / (v_TexRect[2] - v_TexRect[0]);
    o_Colour = getRoundedColor(hsv2rgb(vec4(hueValue, 1, 1, 1)), v_TexCoord);
}
