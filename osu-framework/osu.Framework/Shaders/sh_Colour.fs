#ifdef GL_ES
    precision mediump float;
#endif

varying vec4 v_Colour;

void main(void)
{
	gl_FragColor = v_Colour;
}