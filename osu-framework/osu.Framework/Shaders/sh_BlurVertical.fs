#ifdef GL_ES
    precision mediump float;
#endif

#include "sh_Blur2D.h";

varying vec4 v_Colour;
varying vec2 v_TexCoord;

uniform sampler2D m_Sampler;

uniform vec2 texSize;

uniform int radius;

void main(void)
{
	gl_FragColor = v_Colour * blur(m_Sampler, radius, vec2(0.0, 1.0), v_TexCoord, texSize);
}