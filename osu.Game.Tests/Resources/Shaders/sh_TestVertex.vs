#include "sh_Utils.h"

attribute highp vec2 m_Position;
attribute lowp vec4 m_Colour;
attribute mediump vec2 m_TexCoord;
attribute mediump vec4 m_TexRect;
attribute mediump vec2 m_BlendRange;

varying highp vec2 v_MaskingPosition;
varying lowp vec4 v_Colour;
varying mediump vec2 v_TexCoord;
varying mediump vec4 v_TexRect;
varying mediump vec2 v_BlendRange;

uniform highp mat4 g_ProjMatrix;
uniform highp mat3 g_ToMaskingSpace;

void main(void)
{
    // Transform from screen space to masking space.
    highp vec3 maskingPos = g_ToMaskingSpace * vec3(m_Position, 1.0);
    v_MaskingPosition = maskingPos.xy / maskingPos.z;

    v_Colour = m_Colour;
    v_TexCoord = m_TexCoord;
    v_TexRect = m_TexRect;
    v_BlendRange = m_BlendRange;

    gl_Position = gProjMatrix * vec4(m_Position, 1.0, 1.0);
}

