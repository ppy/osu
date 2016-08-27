attribute vec2 m_Position;
attribute vec2 m_TexCoord;
attribute float m_Time;
attribute vec2 m_Direction;

varying vec4 v_Colour;
varying vec2 v_TexCoord;

uniform mat4 g_ProjMatrix;
uniform float g_FadeClock;
uniform float g_Gravity;

void main(void)
{
	vec2 targetPosition =
		m_Position +
		m_Direction * clamp((g_FadeClock + 100.0) / (m_Time + 100.0), 0.0, 1.0) +
		vec2(0.0, g_Gravity * g_FadeClock * g_FadeClock / 1000000.0);

	gl_Position = g_ProjMatrix * vec4(targetPosition, 1.0, 1.0);
	v_Colour = vec4(1.0, 1.0, 1.0, 1.0 - clamp(g_FadeClock / m_Time, 0.0, 1.0));
	v_TexCoord = m_TexCoord;
}