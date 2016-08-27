#ifdef GL_ES
    precision mediump float;
#endif

vec4 blur(sampler2D tex, int radius, vec2 direction, vec2 texCoord, vec2 texSize)
{
	vec4 sum = vec4(0.0, 0.0, 0.0, 0.0);

    sum += pow(texture2D(tex, texCoord), vec4(2.0));
	//Probably not the smartest way to do this
	//cap at radius = 200 for D3D unrolling
    for (int i = 1; i <= 200; i++)
    {
        sum += pow(texture2D(tex, texCoord + direction * float(i) / texSize), vec4(2.0));
        sum += pow(texture2D(tex, texCoord + direction * float(-i) / texSize), vec4(2.0));
    	if (i == radius)
    		break;
    }
    sum /= float(radius) * 2.0 + 1.0;
    sum = sqrt(sum);

    return sum;
}