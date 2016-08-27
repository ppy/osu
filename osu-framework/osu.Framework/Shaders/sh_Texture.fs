#ifdef GL_ES
    precision mediump float;
#endif

varying vec4 v_Colour;
varying vec2 v_TexCoord;

uniform sampler2D m_Sampler;

void main(void)
{
	gl_FragColor = v_Colour * texture2D(m_Sampler, v_TexCoord, -0.7);
}