#ifdef GL_ES
    precision mediump float;
#endif

const float TWOPI = 6.28318548;
const float PI = 3.14159274;
const float RING_FUDGE_FACTOR = 0.05;

const float GLOW_OVERSHOOT = 0.3;

uniform vec2 m_CenterPos;
uniform float m_OuterRadius;
uniform float m_InnerRadius;
uniform float m_OuterGlowRadius;
uniform float m_InnerGlowRadius;

uniform float m_Alpha;

uniform vec4 m_RingForegroundColour;
uniform vec4 m_RingBackgroundColour;
uniform vec4 m_GlowColour; // Normalized

uniform float m_Progress;

varying vec2 v_Position;

/**
* Returns 1.0 if m_InnerRadius < dist < m_OuterRadius.
*/
float isRing(float dist)
{
	return smoothstep(m_InnerRadius, m_InnerRadius + 1.0, dist)
	       * smoothstep(m_OuterRadius, m_OuterRadius - 1.0, dist);
}

/**
* If m_InnerGlowRadius < dist < m_InnerRadius || m_OuterRadius < dist < m_OuterGlowRadius
* returns the linear interpolation of the glow bewteen those bounds.
* Returns 0 if no glow.
*/
float isGlow(float dist)
{
	return smoothstep(m_InnerGlowRadius, m_InnerRadius, dist) // Before the ring
	       * clamp(1.0 - isRing(dist), 0.0, 1.0) // During the ring
	       * smoothstep(m_OuterGlowRadius, m_OuterRadius, dist); // After the ring
}

void main(void)
{
	// Radius from the center
	float dist = distance(v_Position, m_CenterPos);

	// Top of area is at -PI / PI, bottom is 0
	float angle = atan(v_Position.x - m_CenterPos.x, v_Position.y - m_CenterPos.y);
	float startAngle = (0.5 - m_Progress) * TWOPI;

	// Glow and AA reduction once the ring is too small
	float reduction = smoothstep(PI, PI - GLOW_OVERSHOOT, startAngle);

	float activeRingAmount = smoothstep(startAngle - reduction * RING_FUDGE_FACTOR, startAngle, angle);

	float activeGlowAmount = max(smoothstep(startAngle - GLOW_OVERSHOOT, startAngle + GLOW_OVERSHOOT, angle), // Start
		                         smoothstep(-PI + GLOW_OVERSHOOT, -PI - GLOW_OVERSHOOT, angle)) // Top (left)
	                         * reduction * smoothstep(PI + GLOW_OVERSHOOT / 2.0, PI - GLOW_OVERSHOOT / 2.0, angle); // Top (right)

	activeGlowAmount += max(0.0, (1.0 - activeGlowAmount) * smoothstep(0.9, 1.0, m_Progress));

	// Todo: Probably make darkening uniforms
	vec4 glowColour = isGlow(dist) * (activeGlowAmount + (1.0 - activeGlowAmount) * 0.2) * m_GlowColour;
	vec4 ringColour = isRing(dist) * (activeRingAmount * m_RingForegroundColour + (1.0 - activeRingAmount) * m_RingBackgroundColour);

	gl_FragColor = (ringColour + glowColour) * m_Alpha;

	//Todo: Discard fragments? idk...
}
