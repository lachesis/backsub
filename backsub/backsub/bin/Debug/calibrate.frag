﻿uniform sampler2D SumTx; // Sum of a/n; Same as mean
uniform sampler2D SumSqTx; // Sum of (a/n)^2
uniform sampler2D FrameTx;
uniform float NumFrames;

uniform int Mode;

#define samp(tex) texture2D(tex, gl_TexCoord[0].xy)
#define sq(v) pow(v, vec4(2.0f,2.0f,2.0f,2.0f))
#define V4_ZERO vec4(0f,0f,0f,0f)
#define V4_ONE vec4(1f,1f,1f,1f)
#define clamp10(v) clamp(v, V4_ZERO, V4_ONE)
 
void main()
{
	if(Mode == 0) // Passthrough
		gl_FragColor = samp(FrameTx);
	if(Mode == 1) // Sum of scaled values
		gl_FragColor = clamp10(samp(SumTx) + samp(FrameTx) / NumFrames);
	if(Mode == 2) // SumSquares of scaled values
		gl_FragColor = clamp10(samp(SumSqTx) + sq(samp(FrameTx))/NumFrames);
	//if(Mode == 2) // SumSqVar
	//	gl_FragColor = clamp10(samp(SumSqTx) + sq(samp(FrameTx) - samp(SumTx)));
	if(Mode == 3) // StdDev of scaled values
		gl_FragColor = clamp10(sqrt(samp(SumSqTx) - sq(samp(SumTx))) * 50.0f);
	//	gl_FragColor = sqrt(samp(SumSqTx) / NumFrames);
}