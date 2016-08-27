attribute vec2 m_Position;
attribute vec4 m_Colour;

varying vec4 v_Colour;

uniform mat4 g_ProjMatrix;

void main(void)
{
	gl_Position = g_ProjMatrix * vec4(m_Position.xy, 1.0, 1.0);
	v_Colour = m_Colour;
}